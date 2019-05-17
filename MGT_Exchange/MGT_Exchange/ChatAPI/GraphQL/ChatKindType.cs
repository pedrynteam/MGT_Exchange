using HotChocolate.Types;
using MGT_Exchange.ChatAPI.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.GraphQL
{
    public class ChatKindType : ObjectType<ChatKind>
    {
        protected override void Configure(IObjectTypeDescriptor<ChatKind> descriptor)
        {

        }
    }

    // Leave it empty, HotChocolate will take care of it
    // but sometimes where there is data involved we need to create a transaction to use the input type, so the schema should know it. Error: CommentInfoInput.deliveredAt: Cannot resolve input-type `System.Nullable<System.DateTime>` - Type: CommentInfoInput
    public class ChatKindInputType : InputObjectType<ChatKind>
    {
        protected override void Configure(IInputObjectTypeDescriptor<ChatKind> descriptor)
        {

        }
    }
}
