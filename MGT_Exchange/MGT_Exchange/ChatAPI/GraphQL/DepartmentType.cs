using HotChocolate.Types;
using MGT_Exchange.ChatAPI.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.GraphQL
{
    public class DepartmentType : ObjectType<department>
    {
        protected override void Configure(IObjectTypeDescriptor<department> descriptor)
        {
        }
    }

    // Leave it empty, HotChocolate will take care of it
    public class DepartmentInputType : InputObjectType<department>
    {
        protected override void Configure(IInputObjectTypeDescriptor<department> descriptor)
        {

        }
    }
}
