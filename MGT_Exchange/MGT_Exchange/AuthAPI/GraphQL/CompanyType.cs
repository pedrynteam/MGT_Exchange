using HotChocolate.Types;
using MGT_Exchange.AuthAPI.GraphQL;
using MGT_Exchange.AuthAPI.MVC;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.AuthAPI.GraphQL
{    
    public class CompanyType : ObjectType<company>
    {
        protected override void Configure(IObjectTypeDescriptor<company> descriptor)
        {
            descriptor.Field(t => t.createdAt)
                .Type<DateTimeType>();

            descriptor.Field(t => t.users)    
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
            return context.Service<MVCDbContext>().UserApp.Where(x => x.companyId.Equals(context.Parent<company>().companyId)).Where(q => q.userName.Contains(nameLike));
        }
        else
        {
            return context.Service<MVCDbContext>().UserApp.Where(x => x.companyId.Equals(context.Parent<company>().companyId));
        }
        
    }
    )
    ;
        }
    }

    // Leave it empty, HotChocolate will take care of it
    // but sometimes where there is data involved we need to create a transaction to use the input type, so the schema should know it. Error: CommentInfoInput.deliveredAt: Cannot resolve input-type `System.Nullable<System.DateTime>` - Type: CommentInfoInput
    public class CompanyInputType : InputObjectType<company>
    {
        protected override void Configure(IInputObjectTypeDescriptor<company> descriptor)
        {
            descriptor.Field(t => t.createdAt)    
                .Type<DateTimeType>();

            descriptor.Field(t => t.id)
                .Ignore();
        }
    }
}
