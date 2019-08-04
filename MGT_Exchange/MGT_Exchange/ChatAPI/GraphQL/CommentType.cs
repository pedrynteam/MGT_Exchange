using GreenDonut;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using MGT_Exchange.AuthAPI.GraphQL;
using MGT_Exchange.AuthAPI.MVC;
using MGT_Exchange.ChatAPI.GraphQL;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.GraphQL
{
    public class CommentType : ObjectType<comment>
    {
        /*
        public Task<Person> GetPerson(string id, IResolverContext context, [Service]IPersonRepository repository)
        {
            return context.BatchDataLoader<string, Person>("personByIdBatch", keys => repository.GetPersonBatchAsync(keys)).LoadAsync(id);
        }
        */

        protected override void Configure(IObjectTypeDescriptor<comment> descriptor)
        {
            descriptor.Field(t => t.chat)
                .Type<ChatType>()
                .Name("chat")
                .Resolver(context => context.Service<MVCDbContext>().Chat.FindAsync(context.Parent<comment>().chatId))
                ;            

            descriptor.Field(t => t.seenByAll)
                .Type<BooleanType>()
                .Resolver(async context =>
                {
                    /*
                    var x0 = context.Service<MVCDbContext>().Comment
                    .Where(x => x.CommentId == context.Parent<Comment>().CommentId)
                    .Any(x => x.CommentsInfo.Count() == x.CommentsInfo.Count(y => y.Seen == true))                    
                    ;//*/

                    var x2 = context.Service<MVCDbContext>().CommentInfo
                    .Where(x => x.commentId == 1)
                    .GroupBy(x => x.commentId)
                    .ToDictionary(g => g.Key, g => g.Any(x => x.commentId == 2));


                    //* Version 1
                    IDataLoader<int, Boolean> dataLoader = context.BatchDataLoader<int, Boolean>(
                "CommentSeenById",
                async keys => await
                (from commInfo in context.Service<MVCDbContext>().CommentInfo
                     //where comm.CommentId == context.Parent<Comment>().CommentId
                 where keys.Contains(commInfo.commentId)
                 group commInfo by commInfo.commentId into grp
                 select new
                 {
                     id = grp.Key,
                     participants = grp.Count(), // All participants
                     seen = grp.Count(x => x.seen == true) // Only messages seen
                 }).ToDictionaryAsync(mc => mc.id, mc => (mc.seen == mc.participants) ? true : false)
                    
                );//*/

                    /* Version 2
                    IDataLoader<int, Boolean> dataLoader = context.BatchDataLoader<int, Boolean>(               
                        "CommentSeenById",               
                        async keys => await                                   
                        context.Service<MVCDbContext>().Comment
                        .Include(i => i.CommentsInfo)
                        .Where(x => keys.Contains(x.CommentId))
                        .GroupBy(x => x.CommentId)
                        .ToDictionaryAsync(g => g.Key, g => g.Any(x => x.CommentsInfo.Count() == x.CommentsInfo.Count(y => y.Seen == true)))               
                        ); //*/

                    return await dataLoader.LoadAsync(context.Parent<comment>().commentId);

                }
                );

            descriptor.Field(t => t.user)
                .Type<UserAppType>()            
            .Resolver(async context =>
            {
                /* Use the keys, the Dataloader handles which ones are requested
                 * SELECT [z].[ParticipantId], [z].[ChatId], [z].[IsAdmin], [z].[UserAppId], [z.User].[UserAppId], [z.User].[CompanyId], [z.User].[Email], [z.User].[FirstName], [z.User].[LastName], [z.User].[Nickname], [z.User].[Password], [z.User].[TokenAuth], [z.User].[UserName]
                 * FROM [Participant] AS [z]
                 * LEFT JOIN [UserApp] AS [z.User] ON [z].[UserAppId] = [z.User].[UserAppId]
                 * WHERE ([z].[ChatId] = @__Parent_ChatId_0) AND [z].[UserAppId] IN (N'489ecc19-6b9c-4eab-a83e-7f63b9a4a6c4', N'89214e83-3836-4d1a-95be-8a91f2b58e8c', N'8b704c20-fa29-43e8-a4a2-30f101174cd1', N'48358da4-a9a5-4b31-922e-4eba531ef48e')
                 */
                IDataLoader<string, userApp> dataLoader = context.BatchDataLoader<string, userApp>(
                "UserById",
                async keys => await context.Service<MVCDbContext>().Participant.Include(z => z.user).Where(x => x.chatId == context.Parent<comment>().chatId).Where(q => keys.Contains(q.userAppId)).ToDictionaryAsync(mc => mc.userAppId, mc => mc.user)
                );

                return await dataLoader.LoadAsync(context.Parent<comment>().userAppId);
            });

            descriptor.Field(t => t.commentsInfo)
                .Type<ListType<CommentInfoType>>()
                .Name("commentsInfo")
                .Argument("index", a => a.Type<IntType>())
                .Argument("take", a => a.Type<IntType>())
                .Argument("infoForUserId", a => a.Type<StringType>())
                .Argument("infoForCommentId", a => a.Type<IntType>())
            .Resolver(async context =>
             {
                 int index = context.Argument<int>("index");
                 int take = context.Argument<int>("take");
                 int infoForCommentId = context.Argument<int>("infoForCommentId");
                 string infoForUserId = context.Argument<string>("infoForUserId");

                 IDataLoader<int, List<commentInfo>> dataLoader = context.BatchDataLoader<int, List<commentInfo>>(
                "commentsInfoById",
                async keys =>
                {
                    /* Chain of Command
                     * infoForUserId
                     * infoForCommentId
                     * default
                     * index, take N (This is for specific CommentId because of the Index)
                     * */

                    Dictionary<int, List<commentInfo>> result = new Dictionary<int, List<commentInfo>>();

                    if (!string.IsNullOrEmpty(infoForUserId))
                    {
                        return await context.Service<MVCDbContext>().CommentInfo
                        .Where(x => keys.Contains(x.commentId))
                        .Where(x => x.userAppId.Equals(infoForUserId))
                        .GroupBy(g => g.commentId)
                        .ToDictionaryAsync(d => d.Key, d => d.ToList());
                    }

                    if (infoForCommentId > 0)
                    {
                        result = context.Service<MVCDbContext>().CommentInfo
                        .Where(x => keys.Contains(x.commentId))
                        .Where(x => x.commentId == infoForCommentId)
                        .Where(x => x.commentInfoId > index)
                        .GroupBy(g => g.commentId)
                        .SelectMany(g => g.OrderBy(y => y.commentInfoId)
                        .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }))
                        .Where(y => y.Rank <= take)
                        .GroupBy(g => g.Key)
                        .ToDictionary(g => g.Key, g => g.Select(a => a.Item).ToList())
                        ;

                        return result;
                    }

                    // Default: Order by CommentInfoId, show the N oldest
                    return context.Service<MVCDbContext>().CommentInfo
                        .Where(x => keys.Contains(x.commentId))
                        .GroupBy(g => g.commentId)
                        .SelectMany(g => g.OrderBy(y => y.commentInfoId)
                        .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }))
                        .Where(y => y.Rank <= take)
                        .GroupBy(g => g.Key)
                        .ToDictionary(g => g.Key, g => g.Select(a => a.Item).ToList())
                        ;

                }
                    
                );

                 return await dataLoader.LoadAsync(context.Parent<comment>().commentId);
                 
                 
                 /*
                 ds
                 result = await context.Service<MVCDbContext>().CommentInfo
                              .Where(x => keys.Contains(x.CommentId))
                              .Where(x => x.UserAppId.Equals(infoForUserId))
                              .GroupBy(r => r.CommentId)
                              .ToDictionaryAsync(g => g.Key, g => g.ToList());


                 /* Old Logic
                 int _index = context.Argument<int>("index");

                 int _take = context.Argument<int>("take");
                 _take = _take == 0 ? 10 : _take;
                 // 1. Where index based
                 // 2. Order By
                 // 3. Take 

                 return context.Service<MVCDbContext>().CommentInfo.Where(x => x.CommentId == context.Parent<Comment>().CommentId).Where(x => x.CommentInfoId > _index).OrderByDescending(y => y.CommentInfoId).Take(_take);
                 */
             });

            

            descriptor.Field(t => t.createdAt)
             .Type<DateTimeType>();

            /*
                .Resolver(context =>
                {

                    //var repository = context.Service<MVCDbContext>().CommentInfo;
                    var repository = context.Service<MVCDbContext>().UserApp;

                    


                    //return context.BatchDataLoader<string, Person>("personByIdBatch", keys => repository.GetPersonBatchAsync(keys)).LoadAsync(id);

                    //repository.FindAsync(keyValues: keys, cancellationToken: null)

                    string id = "1";

                    var info = context.BatchDataLoader<string, UserApp>("userByIdBatch", keys => await repository.FindAsync(keyValues: keys).LoadAsync(id);



                    // return context.BatchDataLoader<int, CommentInfo>("commentInfoByIdBatch", keys => repository.FindAsync(keyValues: keys, cancellationToken: null)).LoadAsync(id);

                }
                );*/
            /*.Resolver(context =>
            {

                int _index = context.Argument<int>("index");

                int _take = context.Argument<int>("take");
                _take = _take == 0 ? 10 : _take;
                // 1. Where index based
                // 2. Order By
                // 3. Take 
                return context.Service<MVCDbContext>().CommentInfo.Where(x => x.CommentId == context.Parent<Comment>().CommentId).Where(x => x.CommentInfoId > _index).OrderByDescending(y => y.CommentInfoId).Take(_take);
            }
            )
            ;*/


        }
    }


    // Leave it empty, HotChocolate will take care of it
    // but sometimes where there is data involved we need to create a transaction to use the input type, so the schema should know it. Error: CommentInfoInput.deliveredAt: Cannot resolve input-type `System.Nullable<System.DateTime>` - Type: CommentInfoInput
    public class CommentInputType : InputObjectType<comment>
    {
        protected override void Configure(IInputObjectTypeDescriptor<comment> descriptor)
        {
            descriptor.Field(t => t.seenByAll)
                .Ignore();

        }
    }

}
