using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Types;
using MGT_Exchange.GraphQLActions.Resources;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Models;
using MGT_Exchange.ChatAPI.GraphQL;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using MGT_Exchange.AuthAPI.MVC;
using MGT_Exchange.AuthAPI.GraphQL;
using HotChocolate.Subscriptions;
using System.Diagnostics;

namespace MGT_Exchange.ChatAPI.Transactions
{

    // 1. Create Model: Input type is used for Mutation, it should be included if needed
    public class RetrieveMasterInformationByUser_Input
    {
        public userApp User { get; set; }
        public int ChatsRecentTake { get; set; }
        public int CommentsSeenTake { get; set; }
        public int CommentsUnseenTake { get; set; }
        public int CommentsBeforeUnseenTake { get; set; }
        public int CommentsNewestTake { get; set; }
        public int FindSpecificChatId { get; set; }
    }

    public class RetrieveMasterInformationByUser_InputType : InputObjectType<RetrieveMasterInformationByUser_Input>
    {
        protected override void Configure(IInputObjectTypeDescriptor<RetrieveMasterInformationByUser_Input> descriptor)
        {
            descriptor.Field(t => t.User)
                .Type<UserAppInputType>();

            descriptor.Field(t => t.ChatsRecentTake)
                .Type<NonNullType<IntType>>();

            descriptor.Field(t => t.CommentsSeenTake)
                .Type<NonNullType<IntType>>();

            descriptor.Field(t => t.CommentsBeforeUnseenTake)
                .Type<IntType>();

            descriptor.Field(t => t.CommentsUnseenTake)
                .Type<NonNullType<IntType>>();

            descriptor.Field(t => t.CommentsNewestTake)
                .Type<NonNullType<IntType>>();

            descriptor.Field(t => t.FindSpecificChatId)
                .Type<IntType>();
        }
    }

    // 2. Create Model: Output type is used for Mutation, it should be included if needed
    public class RetrieveMasterInformationByUser_Output
    {
        public resultConfirmation ResultConfirmation { get; set; }
        public List<chat> ChatsRecent { get; set; }
        public List<comment> CommentsUnseen { get; set; }
        public List<comment> CommentsSeen { get; set; }
        public List<comment> CommentsNewest { get; set; }
    }

    public class RetrieveMasterInformationByUser_OutputType : ObjectType<RetrieveMasterInformationByUser_Output>
    {

    }

    // 4. Transaction - Logic Controller
    public class RetrieveMasterInformationByUserTxn
    {

        public RetrieveMasterInformationByUserTxn()
        {
        }

        public async Task<RetrieveMasterInformationByUser_Output> Execute(RetrieveMasterInformationByUser_Input input, MVCDbContext contextFather = null, bool autoCommit = true, IEventSender eventSender = null)
        {
            RetrieveMasterInformationByUser_Output output = new RetrieveMasterInformationByUser_Output();
            output.ResultConfirmation = resultConfirmation.resultBad(_ResultMessage: "TXN_NOT_STARTED");

            // Error handling
            bool error = false; // To Handle Only One Error

            try
            {
                MVCDbContext contextMGT = (contextFather != null) ? contextFather : new MVCDbContext();
                // An using statement is in reality a try -> finally statement, disposing the element in the finally. So we need to take advance of that to create a DBContext inheritance                
                try
                {
                    // DBContext by convention is a UnitOfWork, track changes and commits when SaveChanges is called
                    // Multithreading issue: so if we use only one DBContext, it will track all entities (_context.Customer.Add) and commit them when SaveChanges is called, 
                    // No Matter what transactions or client calls SaveChanges.
                    // Note: Rollback will not remove the entities from the tracker in the context. so better dispose it.

                    //***** 0. Make The Validations - Be careful : Concurrency. Same name can be saved multiple times if called at the exact same time. Better have an alternate database constraint

                    // Call Database to validate that: 1. ChatId exists 2. User Exists 3. User is part of the Company 4. User is Participant of the Chat
                    /*
                    var chatess = await (from chates in contextMGT.Chat
                                        where chates.Participants.Any(y => y.UserAppId.Equals(input.User.UserAppId))
                                            // where keys.Contains(commInfo.CommentId)
                                        group chates by new { chates.ChatId , chates.Name  } into grp
                                 select new Chat
                                 {
                                     ChatId = grp.Key.ChatId,
                                     Name = grp.Key.Name,
                                     // Description = grp.Key.Description,
                                     //IsNewestMessageSeen = grp.Any(x => x.Comments.Where(y => y.ChatId == grp.Key.ChatId).OrderByDescending(o => o.CommentId).Take(1).Any(z => z.SeenByAll == true)),
                                     //FirstUnseenForUser = grp.Where(x => x.Comments.Where(y => y.CommentsInfo.Any(z => z.Seen ==false)).OrderByDescending(o => o   ))
                                 }).ToListAsync();

                    foreach (var cha in chatess)
                    {
                        // Debug.WriteLine("id " + cha.ChatId, "Desc: " + cha.Description + " Last Seen " + cha.IsNewestMessageSeen);
                    }*/

                    /*
                    var chats2 = await contextMGT.Chat
                            .Where(x => x.Participants.Any(y => y.UserAppId.Equals(input.User.UserAppId)))
                            .OrderByDescending(x => x.UpdatedAt)
                            .Take(input.ChatsRecentTake)
                            .Select( ne
                            .ToListAsync();

                    foreach (var cha in chats2)
                    {
                        Debug.WriteLine("id " + cha.ChatId, "Desc: " + cha.Description + " Last Seen " + cha.IsNewestMessageSeen);
                    }
                    */


                    if (input.ChatsRecentTake > 0)
                    {
                        List<chat> chats = new List<chat>();

                        if (input.FindSpecificChatId > 0)
                        {
                            chats = await contextMGT.Chat
                                .Where(x => x.chatId == input.FindSpecificChatId)
                                .Where(x => x.participants.Any(y => y.userAppId.Equals(input.User.userAppId)))
                                .OrderByDescending(x => x.updatedAt)
                                .ToListAsync();
                        }
                        else // Find recent Chats
                        {
                            chats = await contextMGT.Chat
                            .Where(x => x.participants.Any(y => y.userAppId.Equals(input.User.userAppId)))
                            .OrderByDescending(x => x.updatedAt)
                            .Take(input.ChatsRecentTake)
                            .ToListAsync();
                        }
                        
                        if (chats.Count == 0)
                        {
                            error = true;
                            output.ResultConfirmation = resultConfirmation.resultBad(_ResultMessage: "CHAT_NOT_FOUND_ERROR");
                            return output;
                        }
                        
                        output.ChatsRecent = chats;

                        List<int> chatIds = chats
                                .GroupBy(g => g.chatId)
                                .Select(user => user.Key) // extract unique Ids from users    
                                .ToList();

                        if (input.CommentsUnseenTake > 0)
                        {                            
                            // To me, this feels like E=mc^2 .. an incredibly simple solution to a relatively complex problem
                            var rankUnseen = contextMGT.Comment
                                .Where(x => chatIds.Contains(x.chatId))
                                .Where(x => x.commentsInfo.Any(y => y.userAppId.Equals(input.User.userAppId) && y.seen == false))
                                .GroupBy(d => d.chatId)
                                .SelectMany(g => g.OrderBy(y => y.commentId)
                                .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }))
                                .Where(y => y.Rank <= input.CommentsUnseenTake);

                            output.CommentsUnseen = rankUnseen.Select(x => x.Item).OrderBy(o => o.chatId).OrderBy(o => o.chatId).ThenBy(o => o.commentId).ToList();
                            
                            // New Logic hybrid communication. If there are unseen messages show: 5- messages Seen, 2+ messages Unseen. Else: 10 messages unseen                           
                            // Do one extra query to get the N Messages bofere the unseen.
                            // a) If there are unseen messages show: 5- messages Seen
                            if ((output.CommentsUnseen.Count() > 0) && (input.CommentsBeforeUnseenTake > 0))
                            {
                                
                                var rankSeenBefore = contextMGT.Comment
                                .Where(x => chatIds.Contains(x.chatId))
                                .Where(x => x.commentsInfo.Any(y => y.userAppId.Equals(input.User.userAppId) && y.seen == true)) // No need we know it's seen
                                .Where(x => x.commentId < (from inner in contextMGT.Comment where inner.chatId == x.chatId where inner.commentsInfo.Any(y => y.userAppId.Equals(input.User.userAppId) && y.seen == false) select inner.commentId).Min())
                                .GroupBy(d => d.chatId)
                                .SelectMany(g => g.OrderByDescending(y => y.commentId)
                                .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }))
                                .Where(y => y.Rank <= input.CommentsBeforeUnseenTake)
                                ;
                                
                                output.CommentsUnseen = output.CommentsUnseen.Union(rankSeenBefore.Select(x => x.Item).ToList()).OrderBy(x => x.chatId).ThenBy(x => x.commentId).ToList();

                                foreach (var value in rankSeenBefore)
                                {
                                    Debug.WriteLine("Rank Seen Before: " + value.Rank + " Key: " + value.Rank + " ChatId: " + value.Item.chatId + " CommentId: " + value.Item.commentId);
                                }

                            }



                        }

                        if (input.CommentsSeenTake > 0)
                        {
                            // Query N Newest Read Messages                       
                            var CommentsSeen = await contextMGT.Comment
                            .Where(x => chatIds.Contains(x.chatId))
                            .Where(x => x.commentsInfo.Any(y => y.userAppId.Equals(input.User.userAppId) && y.seen == true))
                            .OrderByDescending(y => y.commentId)
                            .Take(input.CommentsSeenTake)
                            .ToListAsync();

                            output.CommentsSeen = CommentsSeen.OrderBy(x => x.chatId).ThenBy(x => x.commentId).ToList();

                            var rank = contextMGT.Comment    
                                .Where(x => chatIds.Contains(x.chatId))
                                .Where(x => x.commentsInfo.Any(y => y.userAppId.Equals(input.User.userAppId) && y.seen == true))
                                .GroupBy(d => d.chatId)    
                                .SelectMany(g => g.OrderByDescending(y => y.commentId)    
                                .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }))    
                                .Where(y => y.Rank <= input.CommentsSeenTake);

                            output.CommentsSeen = rank.Select(x => x.Item).OrderBy(o => o.chatId).ThenBy(o => o.commentId).ToList();
                        }

                        if (input.CommentsNewestTake > 0)
                        {

                            /*
                            // Query N Newest Messages (Seen and Unseen)
                            var CommentsNewest = await contextMGT.Comment
                            .Where(x => chatIds.Contains(x.ChatId))
                            .OrderByDescending(y => y.CommentId)
                            .Take(input.CommentsNewestTake)
                            .ToListAsync();
                            
                            output.CommentsNewest = CommentsNewest.OrderBy(x => x.ChatId).ThenBy(x => x.CommentId).ToList();*/


                            var rank = contextMGT.Comment
                                .Where(x => chatIds.Contains(x.chatId))
                                .GroupBy(d => d.chatId)                   
                                .SelectMany(g => g.OrderByDescending(y => y.commentId)
                                .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }))                                     
                                .Where(y => y.Rank <= input.CommentsNewestTake);
                            
                            /*/foreach (var value in rank)
                            {
                                Debug.WriteLine("Rank: " + value.Rank + " Key: " + value.Rank + " Chat: " + value.Item.ChatId + " Comment: " + value.Item.CommentId);
                            }//*/

                            output.CommentsNewest = rank.Select(x => x.Item).OrderBy(o => o.chatId).ThenBy(o => o.commentId).ToList();

                        }

                        /*


                        var rank0 = contextMGT.Comment
                                .Where(x => chatIds.Contains(x.ChatId))
                                .Where(x => x.CommentId >= 3) // This is the N-5 of the first unseen message
                                .GroupBy(d => d.ChatId)
                                .SelectMany(g => g.OrderBy(y => y.CommentId)
                                .Select((x, i) => new { g.Key, Item = x, Rank = i + 1 }))
                                .Where(y => y.Rank <= input.CommentsNewestTake);



                        Debug.WriteLine("Unseen Limit: ");



                        


                        */

                        
                        // Convert the top N Comments to Seen
                        List<int> toMarkAsSeen = new List<int> { 68, 69, 70, 71 };

                        List<commentInfo> comme = await contextMGT.CommentInfo
                            .Where(x => toMarkAsSeen.Contains(x.commentId))
                            .ToListAsync();

                        foreach (var com in comme)
                        {
                            com.seen = true;
                            com.seenAt = DateTime.UtcNow;
                            contextMGT.Update(com);
                        }
                        await contextMGT.SaveChangesAsync();

                        /*/ Mark them as seen by user
                        List<int> toMarkAsSeen = new List<int> { 1,26};

                        List < CommentInfo > comme = await contextMGT.CommentInfo
                            .Where(x => toMarkAsSeen.Contains(x.CommentInfoId))
                            .ToListAsync();

                        foreach (var com in comme)
                        {
                            com.Seen = true;
                            com.SeenAt = DateTime.UtcNow;
                            contextMGT.Update(com);
                        }

                        await contextMGT.SaveChangesAsync(); //*/




                    }




                    // If there are 20 unread show the N oldest unread
                    // If there are 4 unread show the 4 + (N-4) read
                    // If there are 0 unread show the N read

                    /*
                    // Query Unread Messages
                    var comments = await contextMGT.Comment
                    .Where(x => x.ChatId == context.Parent<Chat>().ChatId)
                    .Where(x => x.CommentsInfo.Any(y => y.UserAppId.Equals(_unreadForUserAppId) && y.Seen == false))
                    //.OrderByDescending(y => y.CommentId)
                    .OrderBy(y => y.CommentId) // Newest messages must be at the top, if there are 12 unread show oldest 10
                    .Take(_take)
                    //.OrderBy(o => o.CommentId) // Newest messages must be at the bottom
                    .ToListAsync();


                    // Query Other Messages (N-1) to complete take
                    _take = _take - comments.Count;

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

                    //***** 6. Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database
                    if (!error)
                    {
                        output.ResultConfirmation = resultConfirmation.resultGood(_ResultMessage: "INFORMATION_SUCESSFULLY_RETRIEVED"); // If OK                        
                    }

                    if ("1".Equals("2")) // This transaction is to Read
                    {
                        //***** 0. Validate if the Chat exists, so return that Chat

                        //***** 1. Create the Chat (Atomic because is same Context DB)

                        //***** 4. Save and Commit to the Database (Atomic because is same Context DB) 
                        if (!error && autoCommit)
                        {
                            await contextMGT.SaveChangesAsync(); // Call it only once so do all other operations first
                        }

                        //***** 5. Execute Send e-mails or other events once the database has been succesfully saved
                        //***** If this task fails, there are options -> 1. Retry multiple times 2. Save the event as Delay, 3.Rollback Database, Re

                        //***** 6. Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database                        

                    }// if (!error)
                }
                finally
                {
                    // If the context Father is null the context was created on his own, so dispose it
                    if (contextMGT != null && contextFather == null)
                    {
                        contextMGT.Dispose();
                    }
                }
            }
            catch (Exception ex) // Main try 
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                string innerError = (ex.InnerException != null) ? ex.InnerException.Message : "";
                System.Diagnostics.Debug.WriteLine("Error Inner: " + innerError);
                output = new RetrieveMasterInformationByUser_Output(); // Restart variable to avoid returning any already saved data
                output.ResultConfirmation = resultConfirmation.resultBad(_ResultMessage: "EXCEPTION", _ResultDetail: ex.Message);
            }
            finally
            {
                // Save Logs if needed
            }

            return output;
        }
    }

}

