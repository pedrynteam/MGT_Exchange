using GreenDonut;
using HotChocolate.Resolvers;
using HotChocolate.Types;
using MGT_Exchange.AuthAPI.GraphQL;
using MGT_Exchange.AuthAPI.MVC;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Data;
using MGT_Exchange.Models;
using MGT_Exchange.ParticipantAPI.GraphQL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.GraphQL
{

    public class ChatType : ObjectType<chat>
    {
        protected override void Configure(IObjectTypeDescriptor<chat> descriptor)
        {

            descriptor.Field(t => t.createdAt)
                         .Type<DateTimeType>();

            descriptor.Field(t => t.closed)
                         .Type<BooleanType>();

            descriptor.Field(t => t.updatedAt)
                         .Type<DateTimeType>();

            descriptor.Field(t => t.closedAt)
                         .Type<DateTimeType>();
            /*
            descriptor.Field(t => t.CommentsInChat)
                .Type<IntType>()
                .Resolver(async context =>
                {

                    IDataLoader<int, int> dataLoader = context.BatchDataLoader<int, int>(
                        "CommentsInChatId",
                        async keys => await
                        context.Service<MVCDbContext>().Comment                        
                        .Where(x => keys.Contains(x.ChatId))
                        .GroupBy(x => x.ChatId)
                        .ToDictionaryAsync(g => g.Key, g => g.Count())
                        );

                    
                    return await dataLoader.LoadAsync(context.Parent<Chat>().ChatId);
                }
                );

            descriptor.Field(t => t.UnseenForUser)
                .Type<IntType>()
                .Argument("userAppId", a => a.Type<StringType>())
                .Resolver(async context =>
                {
                    string userAppId = context.Argument<string>("userAppId");
                    
                    IDataLoader<int, int> dataLoader = context.BatchDataLoader<int, int>(
                        "UnseenForUser",
                        async keys => await
                        context.Service<MVCDbContext>().Comment
                        .Where(x => keys.Contains(x.ChatId))
                        .Where(x => x.CommentsInfo.Any(y => y.Seen == false && y.UserAppId.Equals(userAppId)))
                        .GroupBy(x => x.ChatId)
                        .ToDictionaryAsync(g => g.Key, g => g.Count())
                        );

                    return await dataLoader.LoadAsync(context.Parent<Chat>().ChatId);

                }
                );
                */

            descriptor.Field(t => t.comments)    
                .Type<ListType<CommentType>>()  
                .Name("comments")    
                .Argument("index", a => a.Type<IntType>())    
                .Argument("take", a => a.Type<IntType>())
                .Argument("older", a => a.Type<BooleanType>())                
                .Argument("unseenForUserId", a => a.Type<StringType>())
                .Argument("unseenForUserIdTake", a => a.Type<IntType>())
                .Argument("newestWhenNoUnseenTake", a => a.Type<IntType>())
                .Argument("newestInChatTake", a => a.Type<IntType>())
                .Resolver(async context =>   
                {
                    /* Priority:
                     * 1. newestInChatTake
                     * 2. unseenForUserId, unseenForUserIdTake, newestWhenNoUnseenTake
                     * */

                     // Return X newest messages. Main use: to show the newest message, unseen or not
                    int newestInChatTake = context.Argument<int>("newestInChatTake");

                    // Return X unseen messages for user
                    string unseenForUserId = context.Argument<string>("unseenForUserId");
                    int unseenForUserIdTake = context.Argument<int>("unseenForUserIdTake");
                    int newestWhenNoUnseenTake = context.Argument<int>("newestWhenNoUnseenTake");
                    
                    IDataLoader<int, List<comment>> dataLoaderComm = context.BatchDataLoader<int, List<comment>>(
                    "unseenForUserId",
                    async keys =>
                    {
                        Dictionary<int, List<comment>> result = new Dictionary<int, List<comment>>();

                        if (newestInChatTake > 0) // Max Priority
                        {
                            // To me, this feels like E=mc^2 .. an incredibly simple solution to a relatively complex problem
                            var rankNewest = context.Service<MVCDbContext>().Comment
                                .Where(x => keys.Contains(x.chatId))
                                .GroupBy(d => d.chatId)
                                .SelectMany(g => g.OrderByDescending(y => y.commentId)
                                .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }))
                                .Where(y => y.Rank <= newestInChatTake)
                                .GroupBy(g => g.Key)
                                .ToDictionary(g => g.Key, g => g.Select(a => a.Item).OrderBy(y => y.commentId).ToList())
                                ;
                            
                            result = rankNewest;
                        } // if (newestForUserId > 0) // Max Priority
                        else
                        if (!string.IsNullOrEmpty(unseenForUserId))
                        {

                            // To me, this feels like E=mc^2 .. an incredibly simple solution to a relatively complex problem
                            var rankUnseen = context.Service<MVCDbContext>().Comment
                                .Where(x => keys.Contains(x.chatId))
                                .Where(x => x.commentsInfo.Any(y => y.userAppId.Equals(unseenForUserId) && y.seen == false))
                                .GroupBy(d => d.chatId)
                                .SelectMany(g => g.OrderBy(y => y.commentId)
                                .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }))
                                .Where(y => y.Rank <= unseenForUserIdTake)
                                .GroupBy(g => g.Key)
                                .ToDictionary(g => g.Key, g => g.Select(a => a.Item).ToList())
                                ;

                            // Fix: Microsoft.EntityFrameworkCore.Query:Warning: The LINQ expression 'GroupBy([x].ChatId, [x])' could not be translated and will be evaluated locally.                            

                            if (newestWhenNoUnseenTake == 0)
                            {
                                result = rankUnseen;
                            }
                            else
                            {
                                IReadOnlyList<int> keysNotTo = rankUnseen.Select(x => x.Key).Distinct().ToList();
                                var keysSeen = keys.Except(keysNotTo);

                                // To me, this feels like E=mc^2 .. an incredibly simple solution to a relatively complex problem
                                var rankNewestNoUnseen = context.Service<MVCDbContext>().Comment
                                    .Where(x => keysSeen.Contains(x.chatId))
                                    .GroupBy(d => d.chatId)
                                    .SelectMany(g => g.OrderByDescending(y => y.commentId)
                                    .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }))
                                    .Where(y => y.Rank <= newestWhenNoUnseenTake)
                                    .GroupBy(g => g.Key)
                                    .ToDictionary(g => g.Key, g => g.Select(a => a.Item).OrderBy(y => y.commentId).ToList())
                                    ;
                                
                                result = rankUnseen.Concat(rankNewestNoUnseen).ToDictionary(s => s.Key, s => s.Value);
                            }

                        }// if (!string.IsNullOrEmpty(unseenForUserId))

                        return result;
                    }
                    );
                               
                    return await dataLoaderComm.LoadAsync(context.Parent<chat>().chatId);

/*
                    List<Comment> comments = new List<Comment>();

                    if (!string.IsNullOrEmpty(unseenForUserId))
                    {
                        /*
                        // 
                        // To me, this feels like E=mc^2 .. an incredibly simple solution to a relatively complex problem
                        var rankUnseen = contextMGT.Comment
                            .Where(x => chatIds.Contains(x.ChatId))
                            .Where(x => x.CommentsInfo.Any(y => y.UserAppId.Equals(input.User.UserAppId) && y.Seen == false))
                            .GroupBy(d => d.ChatId)
                            .SelectMany(g => g.OrderBy(y => y.CommentId)
                            .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }))
                            .Where(y => y.Rank <= input.CommentsUnseenTake);

                        output.CommentsUnseen = rankUnseen.Select(x => x.Item).OrderBy(o => o.ChatId).OrderBy(o => o.ChatId).ThenBy(o => o.CommentId).ToList();
                        */
/*
                    }

                    return comments;

/*
                    bool _older = context.Argument<bool>("older");
                    bool _onlyNewest = context.Argument<bool>("onlyNewest");
                    string _unreadForUserAppId = context.Argument<string>("unreadForUserAppId");
                    int _index = context.Argument<int>("index");

                    int _take = context.Argument<int>("take");
                    _take = _take == 0 ? 10 : _take;

                    // 1. Where index based
                    // 2. Order By
                    // 3. Take 

                    /*
                    var query = from comments in context.Service<MVCDbContext>().Comment
                                where comments.ChatId == context.Parent<Chat>().ChatId
                                where comments.CommentId > _index                               
                                orderby ("commentId") descending
                                select comments
                                
                                ;

                    return query.ToListAsync();


                    //*/

                    //            select new CommentInfo { CommentInfoId = 0, CommentId = 0, CreatedAt = DateTime.UtcNow, Delivered = false, Seen = false, UserAppId = user.UserAppId };


                    // return context.Service<MVCDbContext>().Comment.Include(i => i.CommentsInfo)
                    // .Where(x => x.ChatId == context.Parent<Chat>().ChatId).Where(x => x.CommentId > _index)
                    // .OrderByDescending(y => y.CommentId).Take(_take);

                    /*
                    // query is not executed here
                    var comments = from s in context.Service<MVCDbContext>().Comment
                                   where s.ChatId == context.Parent<Chat>().ChatId
                                   //where s.CommentId > _index
                                   select s;

                    if (_older) // Index: 15. Results: 5 to 15  
                    {
                        comments.Where(x => x.CommentId < _index);
                        comments.OrderByDescending(x => x.CommentId);
                    }
                    else // Index: 15. Results: 15 to 25 
                    {
                        comments.Where(x => x.CommentId > _index);
                        comments.OrderBy(x => x.CommentId);
                    }

                    comments.Take(_take);

                    //*/

                    
/*
                    List<Comment> comments = new List<Comment>();

                    if (!string.IsNullOrEmpty(_unreadForUserAppId))
                    {
                        //4
                       /*
                        var cont = context.Service<MVCDbContext>();

                        CommentInfo com = await cont.CommentInfo
                        .Where(x => x.CommentInfoId == 4)
                        .FirstOrDefaultAsync();

                        com.Seen = true;
                        com.SeenAt = DateTime.UtcNow;
                        cont.Update(com);
                        await cont.SaveChangesAsync();
                        */

                        // If there are 20 unread show the N oldest unread
                        // If there are 4 unread show the 4 + (N-4) read
                        // If there are 0 unread show the N read

                        /*/ Query N Oldest Unread Messages
                        var commentsUnread = await context.Service<MVCDbContext>().Comment                        
                        .Where(x => x.ChatId == context.Parent<Chat>().ChatId)
                        .Where(x => x.CommentsInfo.Any(y => y.UserAppId.Equals(_unreadForUserAppId) && y.Seen == false))
                        .OrderBy(y => y.CommentId) // Newest messages must be at the top, if there are 12 unread show oldest 10
                        .Take(_take)
                        .ToListAsync(); //*/

/*
                        // Query N Newest Read Messages                       
                        var commentsRead = await context.Service<MVCDbContext>().Comment
                        .Include(i => i.CommentsInfo)
                        .Where(x => x.ChatId == context.Parent<Chat>().ChatId)
                        .Where(x => x.CommentsInfo.Any(y => y.UserAppId.Equals(_unreadForUserAppId) && y.Seen == true))
                        .OrderByDescending(y => y.CommentId)
                        .Take(_take).OrderBy(o => o.CommentId).ToListAsync();

                        // Combine the two queries so we can get all the following combinations

                        //comments = commentsUnread.Union(commentsRead)
                        //  .OrderBy(t => t.CommentId).ToList();

                        comments = await context.Service<MVCDbContext>().Comment.Where(x => x.ChatId == context.Parent<Chat>().ChatId).OrderByDescending(y => y.CommentId).Take(_take).OrderBy(o => o.CommentId).ToListAsync();


                        /*/
/*                         * comments = await context.Service<MVCDbContext>().Comment
                            .Where(x => x.ChatId == context.Parent<Chat>().ChatId)
                            .Where(x => x.CommentsInfo.Any(y => y.UserAppId.Equals(_unreadForUserAppId) && y.Seen == false))
                            .OrderBy(y => y.CommentId) // Newest messages must be at the top, if there are 12 unread show oldest 10
                            .Take(_take)
                            .ToListAsync(); //*/


                        /*
                        if (_take > 0 )
                        {
                            _index = comments.Min(x => x.CommentId);

                            // Index: 15. Results: 5 to 14
                            comments = await context.Service<MVCDbContext>()
                            .Comment.Where(x => x.ChatId == context.Parent<Chat>().ChatId)
                            .Where(x => x.CommentId < _index)
                            .OrderByDescending(y => y.CommentId)
                            .Take(_take).OrderBy(o => o.CommentId)
                            .ToListAsync();
                        }
                        */
/*
                    }
                    else
                    if (_onlyNewest) // No uses Index, just Take N newest comments
                    {
                        comments = await context.Service<MVCDbContext>().Comment.Include(i => i.CommentsInfo).Where(x => x.ChatId == context.Parent<Chat>().ChatId).OrderByDescending(y => y.CommentId).Take(_take).OrderBy(o => o.CommentId).ToListAsync();               
                    }
                    else
                    if (_older) // Index: 15. Results: 14 to 5  
                    {
                        // Index: 15. Results: 14 to 5 
                        //comments = await context.Service<MVCDbContext>().Comment.Where(x => x.ChatId == context.Parent<Chat>().ChatId).Where(x => x.CommentId < _index).OrderByDescending(y => y.CommentId).Take(_take).ToListAsync();
                        // Index: 15. Results: 5 to 14
                        comments = await context.Service<MVCDbContext>().Comment.Where(x => x.ChatId == context.Parent<Chat>().ChatId).Where(x => x.CommentId < _index).OrderByDescending(y => y.CommentId).Take(_take).OrderBy(o => o.CommentId).ToListAsync();

                    }
                    else // Index: 15. Results: 16 to 25
                    {
                        comments = await context.Service<MVCDbContext>().Comment.Where(x => x.ChatId == context.Parent<Chat>().ChatId).Where(x => x.CommentId > _index).OrderBy(y => y.CommentId).Take(_take).ToListAsync();

                    }


                    // return context.Service<MVCDbContext>().Comment.Where(x => x.ChatId == context.Parent<Chat>().ChatId).Where(x => x.CommentId > _index).OrderByDescending(y => y.CommentId).Take(_take);

                    return comments;
                    */
                }
                )
                ;
            
            descriptor.Field(t => t.participants)    
                            .Type<ListType<ParticipantType>>()  
                            .Name("participants")    
                            .Argument("index", a => a.Type<IntType>())    
                            .Argument("take", a => a.Type<IntType>())    
                            .Resolver(context =>   
                            {

                                int _index = context.Argument<int>("index");

                                int _take = context.Argument<int>("take");
                                _take = _take == 0 ? 10 : _take;
                                // 1. Where index based
                                // 2. Order By
                                // 3. Take 
                                return context.Service<MVCDbContext>().Participant.Where(x => x.chatId == context.Parent<chat>().chatId).Where(x => x.participantId > _index).OrderByDescending(y => y.participantId).Take(_take);
                            }    
                            )    
                            ;

            /* fox foreing keys
            descriptor.Field(t => t.CreatedBy)
                .Type<IdentityUserType>()
                .Name("createdByUser")
                .Resolver(async context =>
                {
                    // UserApp in MVCDbContext, User Account Context in ApplicationDB this for query simplicity and Database security
                    // This database dont contain any users, users are in Users / Account Database
                    return await context.Service<MVCDbContext>().UserApp.Where(x => x.UserAppId == context.Parent<Chat>().UserAppId).FirstOrDefaultAsync();
                    //var userDB = await context.Service<ApplicationDbContext>().Users.Where(x => x.Id == context.Parent<Chat>().UserId).FirstOrDefaultAsync();
                    // UserApp userApp = new UserApp { Id = userDB.Id, Email = userDB.Email, UserName = userDB.UserName };
                    //return userDB; // userApp;
                }
                )
                ;
                */

/*
            descriptor.Field(t => t.ChatStatus)
    .Type<ChatStatusType>()
    //.Name("chatStatus")
    .Resolver(context =>
    {
        return context.Service<MVCDbContext>().ChatStatus.Where(x => x.ChatStatusId == context.Parent<Chat>().ChatStatusId).FirstOrDefaultAsync();
    }
    )
    ;

            descriptor.Field(t => t.ChatKind)
    .Type<ChatKindType>()
    //.Name("chatStatus")
    .Resolver(context =>
    {
        return context.Service<MVCDbContext>().ChatKind.Where(x => x.ChatKindId == context.Parent<Chat>().ChatKindId).FirstOrDefaultAsync();
    }
    )
    ;

*/
        }
    }

    // Leave it empty, HotChocolate will take care of it
    // but sometimes where there is data involved we need to create a transaction to use the input type, so the schema should know it. Error: CommentInfoInput.deliveredAt: Cannot resolve input-type `System.Nullable<System.DateTime>` - Type: CommentInfoInput
    public class ChatInputType : InputObjectType<chat>
    {
        protected override void Configure(IInputObjectTypeDescriptor<chat> descriptor)
        {
            descriptor.Field(t => t.createdAt)
             .Type<DateTimeType>();

            descriptor.Field(t => t.updatedAt)
                         .Type<DateTimeType>();

            descriptor.Field(t => t.closedAt)
                         .Type<DateTimeType>();

            descriptor.Field(t => t.closed)
             .Type<BooleanType>();

        }
    }
}
