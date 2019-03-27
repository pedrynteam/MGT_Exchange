using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.GraphQLActions.Resources
{
    public class ItemKey
    {
        public string Tag { get; set; }
        public string Value { get; set; }

        public ItemKey(string tag, string value)
        {
            Tag = tag;
            Value = value;
        }
    }

    public class ItemKeyType : ObjectType<ItemKey>
    {
        protected override void Configure(IObjectTypeDescriptor<ItemKey> descriptor)
        {
            descriptor.Field(t => t.Tag)
                .Description("The Tag of the Result List")
                ;

            descriptor.Field(t => t.Value)
                .Description("The Value of the Result List")
                ;
        }
    }
}
