using HotChocolate.Language;
using HotChocolate.Subscriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.GraphQLActions.Resources
{
    // This is the main EventMessge Handler by Pedro Angulo 
    public class OnEventMessageDefault<T> : EventMessage
    {
        public OnEventMessageDefault(String eventName, String argumentTag, String argumentValue, T outputType)
            : base(CreateEventDescription(eventName, argumentTag, argumentValue), outputType)
        {
        }

        private static EventDescription CreateEventDescription(String _eventName, String _argumentTag, String _argumentValue)
        {
            ArgumentNode argumentNode = new ArgumentNode(_argumentTag, _argumentValue);
            EventDescription eventDescription = new EventDescription(_eventName, argumentNode);
            return eventDescription;
        }
    }
}
