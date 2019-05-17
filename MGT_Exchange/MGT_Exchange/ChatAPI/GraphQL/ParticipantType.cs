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
    public class ParticipantType : ObjectType<Participant>
    {
        protected override void Configure(IObjectTypeDescriptor<Participant> descriptor)
        {
            descriptor.Field(t => t.Chat)    
                .Type<ChatType>()    
                .Name("chat")    
                .Resolver(context => context.Service<MVCDbContext>().Chat.FindAsync(context.Parent<Participant>().ChatId))    
                ;


            descriptor.Field(t => t.User)
                .Type<UserAppType>()
                .Resolver(context => context.Service<MVCDbContext>().UserApp.FindAsync(context.Parent<Participant>().UserAppId));


        }
    }

    // Leave it empty, HotChocolate will take care of it
    // but sometimes where there is data involved we need to create a transaction to use the input type, so the schema should know it. Error: CommentInfoInput.deliveredAt: Cannot resolve input-type `System.Nullable<System.DateTime>` - Type: CommentInfoInput
    public class ParticipantInputType : InputObjectType<Participant>
    {
        protected override void Configure(IInputObjectTypeDescriptor<Participant> descriptor)
        {

        }
    }
}
