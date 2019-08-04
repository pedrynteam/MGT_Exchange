using HotChocolate.Types;
using MGT_Exchange.AuthAPI.MVC;
using MGT_Exchange.ChatAPI.GraphQL;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.AuthAPI.GraphQL
{
    public class UserAppType : ObjectType<userApp>
    {
        protected override void Configure(IObjectTypeDescriptor<userApp> descriptor)
        {

            descriptor.Field(t => t.company)
    .Type<CompanyType>()
    .Name("company")
    .Resolver(context =>
    {

        //return context.Service<MVCDbContext>().Company.Where(x => x.companyId.Equals(context.Parent<company>().companyId)).FirstOrDefault();
        return context.Service<MVCDbContext>().Company.Where(x => x.companyId == context.Parent<company>().companyId).FirstAsync();
    }
    )
    ;
        }
    }

    // Leave it empty, HotChocolate will take care of it
    public class UserAppInputType : InputObjectType<userApp>
    {
        
        protected override void Configure(IInputObjectTypeDescriptor<userApp> descriptor)
        {
            descriptor.Field(t => t.lastSeen)
                .Type<DateTimeType>();

            descriptor.Field(t => t.id)
                .Ignore();

            descriptor.Field(t => t.active)
                .Ignore();
        }
    }
}
