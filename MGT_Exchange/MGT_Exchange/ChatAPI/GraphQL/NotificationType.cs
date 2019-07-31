using HotChocolate.Types;
using MGT_Exchange.ChatAPI.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.GraphQL
{
    public class NotificationType : ObjectType<notification>
    {
        protected override void Configure(IObjectTypeDescriptor<notification> descriptor)
        {

            descriptor.Field(t => t.createdAt)
                .Type<DateTimeType>();

            descriptor.Field(t => t.seenAt)
                .Type<DateTimeType>();
            
        }
    }

    // Leave it empty, HotChocolate will take care of it
    public class NotificationInputType : InputObjectType<notification>
    {
        protected override void Configure(IInputObjectTypeDescriptor<notification> descriptor)
        {

            descriptor.Field(t => t.createdAt)
                .Type<DateTimeType>();

            descriptor.Field(t => t.seenAt)
                .Type<DateTimeType>();

        }
    }
}
