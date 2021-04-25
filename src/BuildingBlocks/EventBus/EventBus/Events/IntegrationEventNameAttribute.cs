using System;

namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class IntegrationEventNameAttribute : Attribute
    {
        public string EventName { get; private set; }
        
        public IntegrationEventNameAttribute(string eventName)
        {
            EventName = eventName;
        }
    }
}