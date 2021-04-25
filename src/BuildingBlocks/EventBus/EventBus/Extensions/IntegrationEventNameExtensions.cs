using Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Events;

namespace Microsoft.eShopOnContainers.BuildingBlocks.EventBus.Extensions
{
    public static class IntegrationEventNameExtensions
    {
        public static string GetEventName(this IntegrationEvent @event)
        {
            var type = @event.GetType();
            var attributes = type.GetCustomAttributes(false);
            foreach (var attr in attributes)
            {
                //if class contains IntegrationEventNameAttribute take event name from attribute
                if (attr is IntegrationEventNameAttribute eventInfoAttribute)
                    return eventInfoAttribute.EventName;
            }

            return type.Name; //otherwise event name is class name
        }
    }
}