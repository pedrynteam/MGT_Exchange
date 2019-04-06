using HotChocolate.Types;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.AuthAPI.GraphQL
{    
    public class IdentityUserType : ObjectType<IdentityUser>
    {
        protected override void Configure(IObjectTypeDescriptor<IdentityUser> descriptor)
        {
            descriptor.Field(t => t.AccessFailedCount);
            descriptor.Field(t => t.ConcurrencyStamp);
            descriptor.Field(t => t.Email);
            descriptor.Field(t => t.EmailConfirmed);
            descriptor.Field(t => t.Id);
            descriptor.Field(t => t.LockoutEnabled);
            descriptor.Field(t => t.LockoutEnd);
            descriptor.Field(t => t.NormalizedEmail);
            descriptor.Field(t => t.NormalizedUserName);
            descriptor.Field(t => t.PasswordHash).Ignore();
            descriptor.Field(t => t.PhoneNumber);
            descriptor.Field(t => t.PhoneNumberConfirmed);
            descriptor.Field(t => t.SecurityStamp);
            descriptor.Field(t => t.TwoFactorEnabled);
            descriptor.Field(t => t.UserName);
        }
    }

    // Leave it empty, HotChocolate will take care of it
    public class IdentityUserInputType : InputObjectType<IdentityUser>
    {
        protected override void Configure(IInputObjectTypeDescriptor<IdentityUser> descriptor)
        {
            descriptor.Field(t => t.AccessFailedCount).Ignore();
            descriptor.Field(t => t.ConcurrencyStamp).Ignore();
            descriptor.Field(t => t.Email);
            descriptor.Field(t => t.EmailConfirmed).Ignore();
            descriptor.Field(t => t.Id).Ignore();
            descriptor.Field(t => t.LockoutEnabled).Ignore();
            descriptor.Field(t => t.LockoutEnd).Ignore();
            descriptor.Field(t => t.NormalizedEmail).Ignore();
            descriptor.Field(t => t.NormalizedUserName).Ignore();
            descriptor.Field(t => t.PasswordHash);
            descriptor.Field(t => t.PhoneNumber).Ignore();
            descriptor.Field(t => t.PhoneNumberConfirmed).Ignore();
            descriptor.Field(t => t.SecurityStamp).Ignore();
            descriptor.Field(t => t.TwoFactorEnabled).Ignore();
            descriptor.Field(t => t.UserName);
        }
    }
}
