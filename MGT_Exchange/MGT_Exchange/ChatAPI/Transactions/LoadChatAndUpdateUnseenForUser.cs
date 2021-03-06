﻿using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Types;
using MGT_Exchange.GraphQLActions.Resources;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Models;
using MGT_Exchange.ChatAPI.GraphQL;
using Microsoft.EntityFrameworkCore;
using HotChocolate.Subscriptions;
using MGT_Exchange.AuthAPI.MVC;
using System.Collections.Generic;
using MGT_Exchange.AuthAPI.GraphQL;
using Newtonsoft.Json;

namespace MGT_Exchange.ChatAPI.Transactions
{
    // 1. Create Model: Input type is used for Mutation, it should be included if needed
    public class LoadChatAndUpdateUnseenForUserTxn_Input
    {
        public chat Chat { get; set; }
        public userApp User { get; set; }
        public int Take { get; set; }
    }

    public class LoadChatAndUpdateUnseenForUserTxn_InputType : InputObjectType<LoadChatAndUpdateUnseenForUserTxn_Input>
    {
        protected override void Configure(IInputObjectTypeDescriptor<LoadChatAndUpdateUnseenForUserTxn_Input> descriptor)
        {
            descriptor.Field(t => t.Chat)
                .Type<ChatInputType>();

            descriptor.Field(t => t.User)
                .Type<UserAppInputType>();

            descriptor.Field(t => t.Take)
                .Type<IntType>();

        }
    }

    // 2. Create Model: Output type is used for Mutation, it should be included if needed
    public class LoadChatAndUpdateUnseenForUserTxn_Output
    {
        public resultConfirmation ResultConfirmation { get; set; }
        public List<comment> Comments { get; set; }
    }

    public class LoadChatAndUpdateUnseenForUserTxn_OutputType : ObjectType<LoadChatAndUpdateUnseenForUserTxn_Output>
    {

    }

    // 4. Transaction - Logic Controller
    public class LoadChatAndUpdateUnseenForUserTxn
    {

        public LoadChatAndUpdateUnseenForUserTxn()
        {
        }

        public async Task<LoadChatAndUpdateUnseenForUserTxn_Output> Execute(LoadChatAndUpdateUnseenForUserTxn_Input input, MVCDbContext contextFather = null, bool autoCommit = true, IEventSender eventSender = null, IEventRegistry eventRegistry = null)
        {
            LoadChatAndUpdateUnseenForUserTxn_Output output = new LoadChatAndUpdateUnseenForUserTxn_Output();
            output.ResultConfirmation = resultConfirmation.resultBad(_ResultMessage: "TXN_NOT_STARTED");

            // Error handling
            bool error = false; // To Handle Only One Error
            DateTime nowUTC = DateTime.UtcNow;
            List<notification> notifications = new List<notification>();
            List<comment> allSeen = new List<comment>();

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

                    // Call database to validate ChatId and UserId existance or just try to execute the query and if it fails is because one of those is wrong
                    
                    chat chatDb = await contextMGT.Chat
                        .Where(x => x.chatId == input.Chat.chatId)
                        .Where(x => x.participants.Any(y => y.userAppId.Equals(input.User.userAppId)))                        
                        .FirstOrDefaultAsync();

                    if (chatDb == null)
                    {
                        error = true;
                        output.ResultConfirmation = resultConfirmation.resultBad(_ResultMessage: "USER_NOT_AUTHORIZED_ERROR", _ResultDetail: "");
                        return output;
                    }
                    
                    if (!error) //***** 1. Retrieve the unseen Comments (Atomic because is same Context DB)
                    {

                        // Get the N unseen comments if any
                        bool someAllseen = false;
                        
                        output.Comments = await contextMGT.Comment
                            .Where(x => x.chatId == chatDb.chatId)
                            .Where(x => x.commentsInfo.Any(y => y.userAppId.Equals(input.User.userAppId) && y.seen == false))
                            .OrderBy(o => o.commentId)
                            .Take(input.Take)
                            .ToListAsync();

                        if (output.Comments.Count() > 0)
                        {                            
                            // Update the commentsInfo to the context
                            List<int> keys = output.Comments.Select(y => y.commentId).ToList();

                            List<commentInfo> details = await contextMGT.CommentInfo
                                .Where(x => keys.Contains(x.commentId))
                                .Where(x => x.userAppId.Equals(input.User.userAppId))
                                .ToListAsync();
                            
                            details.ForEach(x =>
                            {
                                x.seen = true;
                                x.seenAt = nowUTC;
                            });

                            contextMGT.CommentInfo.UpdateRange(details);

                            // Commit? Yes I need it for the next step
                            await contextMGT.SaveChangesAsync(); //

                            allSeen = await contextMGT.Comment
                                .Include(i => i.commentsInfo)
                                .Where(x => keys.Contains(x.commentId))
                                .Where(x => x.commentsInfo.Count() == x.commentsInfo.Count(y => y.seen == true))
                                .ToListAsync();

                            if (allSeen.Count() > 0)
                            {
                                someAllseen = true;
                                allSeen.ForEach(x => x.seenByAll = true);
                                contextMGT.UpdateRange(allSeen);
                                await contextMGT.SaveChangesAsync();
                            }

                        }
                        else // Get the N newest seen comments if any
                        {
                            output.Comments = await contextMGT.Comment
                            .Where(x => x.chatId == chatDb.chatId)                            
                            .OrderByDescending(o => o.commentId)
                            .Take(input.Take)
                            .OrderBy(o => o.commentId) // Reorder
                            .ToListAsync();
                        }
                        

                        //***** 4. Save and Commit to the Database (Atomic because is same Context DB) 
                        if (!error && autoCommit)
                        {
                            // This commit returns the user from Database and updates input.Comment 
                            await contextMGT.SaveChangesAsync(); // Call it only once so do all other operations first
                        }


                        /*
                        // Save the notifications - No need to?

                        String chatGroup = (chatDb.Participants.Count > 1) ? chatDb.Name : "";
                        */

                        if (someAllseen)
                        {
                            // Send the message for himself too, it's the way to know which message has been seen by all
                            List<comment> CommentsIds = (from ids in allSeen
                                                        select new comment
                                                        {
                                                            commentId = ids.commentId,
                                                            chatId = ids.chatId,
                                                            seenByAll = ids.seenByAll
                                                        }).ToList();

                            // Create a Notification for each user                                                       
                            var queryNotification = from user in contextMGT.Participant
                                                    where user.chatId == chatDb.chatId
                                                    // where user.UserAppId != input.User.UserAppId // Send the message for himself too, because this include previous all seen messages
                                                    select new notification
                                                    {
                                                        notificationId = 0,
                                                        type = "CommentSeen",
                                                        title = "*CommentSeen*",
                                                        subtitle = "",
                                                        body = JsonConvert.SerializeObject(CommentsIds),
                                                        message = "COMMENTS_SEEN",
                                                        toUserAppId = user.userAppId,
                                                        route = "Comment",
                                                        routeAction = "Seen",
                                                        routeId = user.chatId.ToString(),
                                                        createdAt = nowUTC,
                                                        seen = false
                                                    };


                            notifications = queryNotification.ToList();

                            // Save the Notifications to the context - No need to.
                            //contextMGT.Notification.AddRange(notifications);
                        }

                        if (!error && autoCommit)
                        {
                            // This commit returns the user from Database and updates input.Comment 
                            await contextMGT.SaveChangesAsync(); // Call it only once so do all other operations first
                        }

                        //***** 5. Execute Send e-mails or other events once the database has been succesfully saved
                        //***** If this task fails, there are options -> 1. Retry multiple times 2. Save the event as Delay, 3.Rollback Database, Re

                        //***** 6. Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database
                        output.ResultConfirmation = resultConfirmation.resultGood(_ResultMessage: "COMMENTS_SUCESSFULLY_SEEN"); // If OK                        
                        
                    }// if (!error)

                    if (!error) // Send the notification to the subscriptors
                    {
                        // Send the notification to each user * New Logic *                        
                        foreach (var notification in notifications)
                        {
                            var result = eventSender.SendAsync(new OnEventMessageDefault<notification>(eventName: "onNotificationToUser", argumentTag: "userAppId", argumentValue: notification.toUserAppId, outputType: notification));
                        }
                        
                    }

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
                output = new LoadChatAndUpdateUnseenForUserTxn_Output(); // Restart variable to avoid returning any already saved data
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

