using HotChocolate.Types;
using MGT_Exchange.AuthAPI.GraphQL;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.GraphQL
{    
    public class CompanyType : ObjectType<Company>
    {
        protected override void Configure(IObjectTypeDescriptor<Company> descriptor)
        {
            // descriptor.Field(t => t.Created)                    .Type<DateTimeType>();
            descriptor.Field(t => t.Users)    
                .Type<ListType<UserAppType>>()    
                .Name("users")
                .Argument("nameLike", a => a.Type<StringType>())                
    .Resolver(context =>
    {
        string nameLike = context.Argument<String>("nameLike");

        // Pending make the query configurable to add more Where and at the end run the query
        // Pending don't return all the user values to customer like id, token, password

        if (!String.IsNullOrEmpty(nameLike))
        {
            //return context.Service<MVCDbContext>().UserApp.Where(x => x.CompanyId == context.Parent<Company>().CompanyId).Where(q => q.UserName.Contains(nameLike));
            return context.Service<MVCDbContext>().UserApp.Where(x => x.CompanyId.Equals(context.Parent<Company>().CompanyId)).Where(q => q.UserName.Contains(nameLike));
        }
        else
        {
            return context.Service<MVCDbContext>().UserApp.Where(x => x.CompanyId.Equals(context.Parent<Company>().CompanyId));
        }
        
    }
    )
    ;
        }
    }

    // Leave it empty, HotChocolate will take care of it
    public class CompanyInputType : InputObjectType<Company>
    {
        protected override void Configure(IInputObjectTypeDescriptor<Company> descriptor)
        {
            //descriptor.Field(t => t.Created)                .Type<DateTimeType>();
        }
    }
}
