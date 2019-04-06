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
    public class UserAppType : ObjectType<UserApp>
    {
        protected override void Configure(IObjectTypeDescriptor<UserApp> descriptor)
        {

            descriptor.Field(t => t.Company)
    .Type<CompanyType>()
    .Name("company")
    .Resolver(context =>
    {

        return context.Service<MVCDbContext>().Company.Where(x => x.CompanyId.Equals(context.Parent<Company>().CompanyId)).FirstOrDefault();
    }
    )
    ;
        }
    }

    // Leave it empty, HotChocolate will take care of it
    public class UserAppInputType : InputObjectType<UserApp>
    {

    }
}
