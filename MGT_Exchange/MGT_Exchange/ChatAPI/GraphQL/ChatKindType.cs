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
    public class ChatKindInputType : InputObjectType<ChatKind>
    {

    }
}
