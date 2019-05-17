using HotChocolate.Subscriptions;
using HotChocolate.Types;
using MGT_Exchange.ChatAPI.GraphQL;
using MGT_Exchange.ChatAPI.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.GraphQLActions
{
    public class GraphQLSubscription
    {
        public GraphQLSubscription()
        {
        }

        public Comment OnCommentAddedToChat(string chatId, IEventMessage message)
        {
            return (Comment)message.Payload;
        }

        public Notification OnNotificationToUser(string userAppId, IEventMessage message)
        {
            return (Notification)message.Payload;
        }

    }

    public class GraphQLSubscriptionType : ObjectType<GraphQLSubscription>
    {
        protected override void Configure(IObjectTypeDescriptor<GraphQLSubscription> descriptor)
        {
            descriptor.Field(t => t.OnCommentAddedToChat(default, default))
                .Type<NonNullType<CommentType>>()
                .Argument("chatId", arg => arg.Type<NonNullType<StringType>>());

            descriptor.Field(t => t.OnNotificationToUser(default, default))
                .Type<NonNullType<NotificationType>>()
                .Argument("userAppId", arg => arg.Type<NonNullType<StringType>>());
        }
        
    }

}
