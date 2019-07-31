using HotChocolate.Types;
using MGT_Exchange.AuthAPI.GraphQL;
using MGT_Exchange.ChatAPI.GraphQL;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ParticipantAPI.GraphQL
{
    public class ParticipantType : ObjectType<participant>
    {
        protected override void Configure(IObjectTypeDescriptor<participant> descriptor)
        {
            descriptor.Field(t => t.chat)    
                .Type<ChatType>()    
                .Name("chat")    
                .Resolver(context => context.Service<MVCDbContext>().Chat.FindAsync(context.Parent<participant>().chatId))    
                ;


            descriptor.Field(t => t.user)
                .Type<UserAppType>()
                .Resolver(context => context.Service<MVCDbContext>().UserApp.FindAsync(context.Parent<participant>().userAppId));


        }
    }

    // Leave it empty, HotChocolate will take care of it
    // but sometimes where there is data involved we need to create a transaction to use the input type, so the schema should know it. Error: CommentInfoInput.deliveredAt: Cannot resolve input-type `System.Nullable<System.DateTime>` - Type: CommentInfoInput
    public class ParticipantInputType : InputObjectType<participant>
    {
        protected override void Configure(IInputObjectTypeDescriptor<participant> descriptor)
        {

        }
    }
}
