using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBusServiceBus
{
    using System;

    public interface IServiceBusPersisterConnection : IDisposable
    {
        ServiceBusClient TopicClient { get; }
        ServiceBusAdministrationClient AdministrationClient { get; }
    }
}