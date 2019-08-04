using HotChocolate.Types;
using MGT_Exchange.ChatAPI.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.GraphQL
{
    public class GroupType : ObjectType<groupof>
    {
        protected override void Configure(IObjectTypeDescriptor<groupof> descriptor)
        {
        }
    }

    // Leave it empty, HotChocolate will take care of it
    public class GroupInputType : InputObjectType<groupof>
    {
        protected override void Configure(IInputObjectTypeDescriptor<groupof> descriptor)
        {

        }
    }
}
