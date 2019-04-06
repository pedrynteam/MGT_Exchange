using HotChocolate.Types;
using MGT_Exchange.AuthAPI.GraphQL;
using MGT_Exchange.ChatAPI.GraphQL;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.GraphQL
{
    public class CommentType : ObjectType<Comment>
    {
        protected override void Configure(IObjectTypeDescriptor<Comment> descriptor)
        {
            descriptor.Field(t => t.Chat)
                .Type<ChatType>()
                .Name("chat")
                .Resolver(context => context.Service<MVCDbContext>().Chat.FindAsync(context.Parent<Comment>().ChatId))
                ;

            descriptor.Field(t => t.User)
                .Type<UserAppType>()                
                .Resolver(context => context.Service<MVCDbContext>().UserApp.FindAsync(context.Parent<Comment>().UserAppId))                ;

            descriptor.Field(t => t.Created)    
                .Type<DateTimeType>();
        }
    }


    // Leave it empty, HotChocolate will take care of it
    public class CommentInputType : InputObjectType<Comment>
    {
        protected override void Configure(IInputObjectTypeDescriptor<Comment> descriptor)
        {

            descriptor.Field(t => t.Created)
                .Type<DateTimeType>();

        }
    }

}
