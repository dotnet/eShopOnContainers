using Autofac;
using Microsoft.Azure.ServiceBus;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Abstractions;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBusRabbitMQ;
using Microsoft.eShopOnContainers.BuildingBlocks.EventBusServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using Serilog;

namespace Ordering.BackgroundTasks.Extensions
{
    public static class CustomExtensionMethods
    {
        public static IServiceCollection AddCustomHealthCheck(this IServiceCollection services, IConfiguration configuration)
        {
            var hcBuilder = services.AddHealthChecks();

            hcBuilder.AddCheck("self", () => HealthCheckResult.Healthy());

            hcBuilder.AddSqlServer(
                    configuration["ConnectionString"],
                    name: "OrderingTaskDB-check",
                    tags: new string[] { "orderingtaskdb" });

            if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                hcBuilder.AddAzureServiceBusTopic(
                        configuration["EventBusConnection"],
                        topicName: "eshop_event_bus",
                        name: "orderingtask-servicebus-check",
                        tags: new string[] { "servicebus" });
            }
            else
            {
                hcBuilder.AddRabbitMQ(
                        $"amqp://{configuration["EventBusConnection"]}",
                        name: "orderingtask-rabbitmqbus-check",
                        tags: new string[] { "rabbitmqbus" });
            }

            return services;
        }

        public static IServiceCollection AddEventBus(this IServiceCollection services, IConfiguration configuration)
        {
            var subscriptionClientName = configuration["SubscriptionClientName"];

            if (configuration.GetValue<bool>("AzureServiceBusEnabled"))
            {
                services.AddSingleton<IServiceBusPersisterConnection>(sp =>
                {
                    var serviceBusConnectionString = configuration["EventBusConnection"];
                    var serviceBusConnection = new ServiceBusConnectionStringBuilder(serviceBusConnectionString);

                    return new DefaultServiceBusPersisterConnection(serviceBusConnection, subscriptionClientName);
                });

                services.AddSingleton<IEventBus, EventBusServiceBus>(sp =>
                {
                    var serviceBusPersisterConnection = sp.GetRequiredService<IServiceBusPersisterConnection>();
                    var iLifetimeScope = sp.GetRequiredService<ILifetimeScope>();
                    var logger = sp.GetRequiredService<ILogger<EventBusServiceBus>>();
                    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                    return new EventBusServiceBus(serviceBusPersisterConnection, logger, eventBusSubcriptionsManager, iLifetimeScope);
                });
            }
            else
            {
                services.AddSingleton<IRabbitMQPersistentConnection>(sp =>
                {
                    var logger = sp.GetRequiredService<ILogger<DefaultRabbitMQPersistentConnection>>();

                    var factory = new ConnectionFactory()
                    {
                        HostName = configuration["EventBusConnection"],
                        DispatchConsumersAsync = true
                    };

                    if (!string.IsNullOrEmpty(configuration["EventBusUserName"]))
                    {
                        factory.UserName = configuration["EventBusUserName"];
                    }

                    if (!string.IsNullOrEmpty(configuration["EventBusPassword"]))
                    {
                        factory.Password = configuration["EventBusPassword"];
                    }

                    var retryCount = 5;

                    if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
                    {
                        retryCount = int.Parse(configuration["EventBusRetryCount"]);
                    }

                    return new DefaultRabbitMQPersistentConnection(factory, logger, retryCount);
                });

                services.AddSingleton<IEventBus, EventBusRabbitMQ>(sp =>
                {
                    var rabbitMQPersistentConnection = sp.GetRequiredService<IRabbitMQPersistentConnection>();
                    var serviceScopeFactory = sp.GetRequiredService<IServiceScopeFactory>();
                    var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                    var eventBusSubcriptionsManager = sp.GetRequiredService<IEventBusSubscriptionsManager>();

                    var retryCount = 5;

                    if (!string.IsNullOrEmpty(configuration["EventBusRetryCount"]))
                    {
                        retryCount = int.Parse(configuration["EventBusRetryCount"]);
                    }

                    return new EventBusRabbitMQ(rabbitMQPersistentConnection,
                        logger,
                        serviceScopeFactory,
                        eventBusSubcriptionsManager,
                        subscriptionClientName,
                        retryCount);
                });
            }

            services.AddSingleton<IEventBusSubscriptionsManager, InMemoryEventBusSubscriptionsManager>();

            return services;
        }

        public static ILoggingBuilder UseSerilog(this ILoggingBuilder builder, IConfiguration configuration)
        {
            var seqServerUrl = configuration["Serilog:SeqServerUrl"];
            var logstashUrl = configuration["Serilog:LogstashgUrl"];

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .Enrich.WithProperty("ApplicationContext", Program.AppName)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)
                .WriteTo.Http(string.IsNullOrWhiteSpace(logstashUrl) ? "http://logstash:8080" : logstashUrl)
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            return builder;
        }
    }
}
