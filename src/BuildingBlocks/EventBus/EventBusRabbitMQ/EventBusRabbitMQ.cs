using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBusRabbitMQ
{
    public class EventBusRabbitMQ : IEventBus, IDisposable
    {
        private const string BrokerName = "eshop_event_bus";

        private readonly IRabbitMQPersistentConnection _persistentConnection;
        private readonly ILogger<EventBusRabbitMQ> _logger;
        private readonly IEventBusSubscriptionsManager _subsManager;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly int _retryCount;

        private IModel _consumerChannel;
        private string _queueName;

        public EventBusRabbitMQ(IRabbitMQPersistentConnection persistentConnection,
            ILogger<EventBusRabbitMQ> logger,
            IServiceScopeFactory serviceScopeFactory,
            IEventBusSubscriptionsManager subsManager,
            string queueName = null,
            int retryCount = 5)
        {
            _persistentConnection = persistentConnection ?? throw new ArgumentNullException(nameof(persistentConnection));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _subsManager = subsManager ?? new InMemoryEventBusSubscriptionsManager();
            _queueName = queueName;
            _consumerChannel = CreateConsumerChannel();
            _serviceScopeFactory = serviceScopeFactory;
            _retryCount = retryCount;
            _subsManager.OnEventRemoved += SubsManager_OnEventRemoved;
        }

        private void SubsManager_OnEventRemoved(object sender, string eventName)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueUnbind(_queueName, BrokerName, eventName);

                if (!_subsManager.IsEmpty)
                {
                    return;
                }

                _queueName = string.Empty;
                _consumerChannel.Close();
            }
        }

        public void Publish(IntegrationEvent @event)
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            var policy = Policy.Handle<BrokerUnreachableException>()
                .Or<SocketException>()
                .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                    (ex, time) =>
                    {
                        _logger.LogWarning(ex,
                            "Could not publish event: {EventId} after {Timeout}s ({ExceptionMessage})", @event.Id,
                            $"{time.TotalSeconds:n1}", ex.Message);
                    });

            var eventName = @event.GetType().Name;

            _logger.LogTrace("Creating RabbitMQ channel to publish event: {EventId} ({EventName})", @event.Id,
                eventName);

            using (var channel = _persistentConnection.CreateModel())
            {
                _logger.LogTrace("Declaring RabbitMQ exchange to publish event: {EventId}", @event.Id);

                channel.ExchangeDeclare(BrokerName, "direct");

                var body = JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType(), new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                policy.Execute(() =>
                {
                    var properties = channel.CreateBasicProperties();
                    properties.DeliveryMode = 2;

                    _logger.LogTrace("Publishing event to RabbitMQ: {EventId}", @event.Id);

                    channel.BasicPublish(
                        BrokerName,
                        eventName,
                        true,
                        properties,
                        body);
                });
            }
        }

        public void SubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
            _logger.LogInformation("Subscribing to dynamic event {EventName} with {EventHandler}", eventName,
                typeof(TH).GetGenericTypeName());

            DoInternalSubscription(eventName);

            _subsManager.AddDynamicSubscription<TH>(eventName);

            StartBasicConsume();
        }

        public void Subscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();

            DoInternalSubscription(eventName);

            _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName,
                typeof(TH).GetGenericTypeName());
            _subsManager.AddSubscription<T, TH>();

            StartBasicConsume();
        }

        private void DoInternalSubscription(string eventName)
        {
            var containsKey = _subsManager.HasSubscriptionsForEvent(eventName);

            if (containsKey)
            {
                return;
            }

            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            using (var channel = _persistentConnection.CreateModel())
            {
                channel.QueueBind(_queueName, BrokerName, eventName);
            }
        }

        public void Unsubscribe<T, TH>() where T : IntegrationEvent where TH : IIntegrationEventHandler<T>
        {
            var eventName = _subsManager.GetEventKey<T>();

            _logger.LogInformation("Unsubscribing from event {EventName}", eventName);
            _subsManager.RemoveSubscription<T, TH>();
        }

        public void UnsubscribeDynamic<TH>(string eventName) where TH : IDynamicIntegrationEventHandler
        {
            _subsManager.RemoveDynamicSubscription<TH>(eventName);
        }

        public void Dispose()
        {
            _consumerChannel?.Dispose();
            _subsManager.Clear();
        }

        private void StartBasicConsume()
        {
            _logger.LogTrace("Starting RabbitMQ basic consume");

            if (_consumerChannel != null)
            {
                var consumer = new AsyncEventingBasicConsumer(_consumerChannel);

                consumer.Received += Consumer_Received;

                _consumerChannel.BasicConsume(
                    _queueName,
                    false,
                    consumer);
            }
            else
            {
                _logger.LogError("StartBasicConsume can't call on _consumerChannel == null");
            }
        }

        private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
        {
            var eventName = eventArgs.RoutingKey;
            var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

            try
            {
                if (message.ToLowerInvariant().Contains("throw-fake-exception"))
                {
                    throw new InvalidOperationException($"Fake exception requested: \"{message}\"");
                }

                await ProcessEvent(eventName, message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "----- ERROR Processing message \"{Message}\"", message);
            }

            // Even on exception we take the message off the queue.
            // in a REAL WORLD app this should be handled with a Dead Letter Exchange (DLX). 
            // For more information see: https://www.rabbitmq.com/dlx.html
            _consumerChannel.BasicAck(eventArgs.DeliveryTag, false);
        }

        private IModel CreateConsumerChannel()
        {
            if (!_persistentConnection.IsConnected)
            {
                _persistentConnection.TryConnect();
            }

            _logger.LogTrace("Creating RabbitMQ consumer channel");

            var channel = _persistentConnection.CreateModel();

            channel.ExchangeDeclare(BrokerName, "direct");
            channel.QueueDeclare(_queueName,
                true,
                false,
                false,
                null);

            channel.CallbackException += (sender, ea) =>
            {
                _logger.LogWarning(ea.Exception, "Recreating RabbitMQ consumer channel");
                _consumerChannel.Dispose();
                _consumerChannel = CreateConsumerChannel();

                StartBasicConsume();
            };

            return channel;
        }

        private async Task ProcessEvent(string eventName, string message)
        {
            _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

            if (_subsManager.HasSubscriptionsForEvent(eventName))
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var subscriptions = _subsManager.GetHandlersForEvent(eventName);

                    foreach (var subscription in subscriptions)
                    {
                        if (subscription.IsDynamic)
                        {
                            if (!(scope.ServiceProvider.GetService(subscription.HandlerType) is
                                IDynamicIntegrationEventHandler handler))
                            {
                                continue;
                            }

                            using dynamic eventData = JsonDocument.Parse(message);

                            await Task.Yield();
                            await handler.Handle(eventData);
                        }
                        else
                        {
                            var handler = scope.ServiceProvider.GetService(subscription.HandlerType);
                            if (handler == null)
                            {
                                continue;
                            }

                            var eventType = _subsManager.GetEventTypeByName(eventName);
                            var integrationEvent = JsonSerializer.Deserialize(message, eventType,
                                new JsonSerializerOptions
                                {
                                    PropertyNameCaseInsensitive = true
                                });
                            var concreteType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);

                            await Task.Yield();
                            await (Task) concreteType.GetMethod("Handle").Invoke(handler, new[]
                            {
                                integrationEvent
                            });
                        }
                    }
                }
            }
            else
            {
                _logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
            }
        }
    }
}