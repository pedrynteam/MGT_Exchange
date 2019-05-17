using HotChocolate.Types;
using MGT_Exchange.ChatAPI.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.GraphQL
{
    public class NotificationType : ObjectType<Notification>
    {
        protected override void Configure(IObjectTypeDescriptor<Notification> descriptor)
        {

            descriptor.Field(t => t.CreatedAt)
                .Type<DateTimeType>();

            descriptor.Field(t => t.SeenAt)
                .Type<DateTimeType>();
            
        }
    }

    // Leave it empty, HotChocolate will take care of it
    public class NotificationInputType : InputObjectType<Notification>
    {
        protected override void Configure(IInputObjectTypeDescriptor<Notification> descriptor)
        {

            descriptor.Field(t => t.CreatedAt)
                .Type<DateTimeType>();

            descriptor.Field(t => t.SeenAt)
                .Type<DateTimeType>();

        }
    }
}
