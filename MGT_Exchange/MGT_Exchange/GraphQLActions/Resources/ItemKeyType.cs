using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.GraphQLActions.Resources
{
    public class itemKey
    {
        public string tag { get; set; }
        public string value { get; set; }

        public itemKey(string _tag, string _value)
        {
            tag = _tag;
            value = _value;
        }
    }

    public class ItemKeyType : ObjectType<itemKey>
    {
        protected override void Configure(IObjectTypeDescriptor<itemKey> descriptor)
        {
            descriptor.Field(t => t.tag)
                .Description("The Tag of the Result List")
                ;

            descriptor.Field(t => t.value)
                .Description("The Value of the Result List")
                ;
        }
    }
}
