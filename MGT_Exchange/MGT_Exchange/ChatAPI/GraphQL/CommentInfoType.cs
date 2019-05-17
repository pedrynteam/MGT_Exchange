using GreenDonut;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using MGT_Exchange.AuthAPI.GraphQL;
using MGT_Exchange.AuthAPI.MVC;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.GraphQL
{

    public class CommentInfoType : ObjectType<CommentInfo>
    {
        protected override void Configure(IObjectTypeDescriptor<CommentInfo> descriptor)
        {

            descriptor.Field(t => t.CreatedAt)
                .Type<DateTimeType>();

            descriptor.Field(t => t.DeliveredAt)
                .Type<DateTimeType>();

            descriptor.Field(t => t.SeenAt)
                .Type<DateTimeType>();

            descriptor.Field(t => t.User)    
                .Type<UserAppType>()
                .Resolver(async context =>
                {
                    /* Use the keys, the Dataloader handles which ones are requested
                     * SELECT [q].[UserAppId], [q].[CompanyId], [q].[Email], [q].[FirstName], [q].[LastName], [q].[LastSeen], [q].[Nickname], [q].[Password], [q].[TokenAuth], [q].[UserName]
                     * FROM [UserApp] AS [q]
                     * WHERE [q].[UserAppId] IN (N'41bb2122-0eb9-4f28-a75c-65f008d150d3', N'0752a9ae-efc4-43fb-a712-66f39bf87c85', N'008c4bd8-0280-47ac-8e9c-048eec043b6e')
                     * ORDER BY [q].[UserAppId]
                     */
                    IDataLoader<string, UserApp> dataLoader = context.BatchDataLoader<string, UserApp>(
                    "CommentInfoUserById",
                    async keys => {                        
                        return await context.Service<MVCDbContext>().UserApp                        
                        .Where(q => keys.Contains(q.UserAppId))
                        .GroupBy(g => g.UserAppId)
                        .ToDictionaryAsync(d => d.Key, d => d.FirstOrDefault());
                    }
                    );

                    return await dataLoader.LoadAsync(context.Parent<CommentInfo>().UserAppId);
                });

        }
    }

    // Leave it empty, HotChocolate will take care of it. 
    // but sometimes where there is data involved we need to create a transaction to use the input type, so the schema should know it. Error: CommentInfoInput.deliveredAt: Cannot resolve input-type `System.Nullable<System.DateTime>` - Type: CommentInfoInput
    public class CommentInfoInputType : InputObjectType<CommentInfo>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CommentInfo> descriptor)
        {

            descriptor.Field(t => t.CreatedAt)
                .Type<DateTimeType>();

            descriptor.Field(t => t.DeliveredAt)
                .Type<DateTimeType>();

            descriptor.Field(t => t.SeenAt)
                .Type<DateTimeType>();
        }
    }

}
