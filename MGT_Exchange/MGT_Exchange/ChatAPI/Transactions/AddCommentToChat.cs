using System;
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

namespace MGT_Exchange.ChatAPI.Transactions
{    
    // 1. Create Model: Input type is used for Mutation, it should be included if needed
    public class AddCommentToChatTxn_Input
    {
        public Comment Comment { get; set; }
    }

    public class AddCommentToChatTxn_InputType : InputObjectType<AddCommentToChatTxn_Input>
    {
        protected override void Configure(IInputObjectTypeDescriptor<AddCommentToChatTxn_Input> descriptor)
        {
            descriptor.Field(t => t.Comment)
                .Type<CommentInputType>();
        }
    }

    // 2. Create Model: Output type is used for Mutation, it should be included if needed
    public class AddCommentToChatTxn_Output
    {
        public ResultConfirmation ResultConfirmation { get; set; }
        public Comment Comment { get; set; }
    }

    public class AddCommentToChatTxn_OutputType : ObjectType<AddCommentToChatTxn_Output>
    {

    }

    // 4. Transaction - Logic Controller
    public class AddCommentToChatTxn
    {

        public AddCommentToChatTxn()
        {
        }

        public async Task<AddCommentToChatTxn_Output> Execute(AddCommentToChatTxn_Input input, MVCDbContext contextFather = null, bool autoCommit = true, IEventSender eventSender = null, IEventRegistry eventRegistry = null)
        {
            AddCommentToChatTxn_Output output = new AddCommentToChatTxn_Output();
            output.ResultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "TXN_NOT_STARTED");

            // Error handling
            bool error = false; // To Handle Only One Error
            DateTime nowUTC = DateTime.UtcNow;
            List<Notification> notifications = new List<Notification>();

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
                    /*
                    Chat chatDb = await contextMGT.Chat.Where(x => x.ChatId == input.Comment.ChatId).FirstOrDefaultAsync();

                    if (chatDb == null)
                    {
                        error = true;
                        output.ResultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "CHAT_DOES_NOT_EXIST_ERROR", _ResultDetail: "");
                    }
                    */

                    // Call Database to validate that: 1. ChatId exists 2. User Exists 3. User is part of the Company 4. User is Participant of the Chat

                    
                    Chat chatDb0 = await contextMGT.Chat.Include(o => o.Participants)
                        .Where(x => x.ChatId == input.Comment.ChatId)
                        .Where(x => x.Participants.Any(y => y.UserAppId.Equals(input.Comment.UserAppId)))
                        .FirstOrDefaultAsync();


                    Chat chatDb = await contextMGT.Chat
                        .Where(x => x.ChatId == input.Comment.ChatId)
                        .Where(x => x.Participants.Any(y => y.UserAppId.Equals(input.Comment.UserAppId)))
                        .Include(o => o.Participants)
                        //.ThenInclude(i => i.User)                        
                        .FirstOrDefaultAsync();

                    


                    /*
                     var blogs = context.Blogs
        .Include(blog => blog.Posts)
            .ThenInclude(post => post.Author)
                .ThenInclude(author => author.Photo)
        .ToList();
        */

                    //.Where(x => x.ChatId == input.Comment.ChatId)
                    ; // 1. ChatId exists

                    if (chatDb == null)
                    {
                        error = true;
                        output.ResultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "USER_NOT_AUTHORIZED");
                    }

                    // Find and validate user. Future: Create a transaction

                    UserApp userApp = await contextMGT.UserApp.Where(x => x.UserAppId.Equals(input.Comment.UserAppId)).FirstOrDefaultAsync();
                    if (userApp == null)
                    {
                        error = true;
                        output.ResultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "USER_NOT_FOUND");
                    }


                    if (!error)
                    {
                        //***** 1. Create the Comment (Atomic because is same Context DB)

                        // Update the Chat
                        chatDb.UpdatedAt = nowUTC;
                        contextMGT.Chat.Update(chatDb);

                        // Create the Chat To Save
                        input.Comment.CommentId = 0;
                        input.Comment.CreatedAt = DateTime.UtcNow;
                        
                        // Create and Save the CommentInfo for each user. 

                        var query = from user in chatDb.Participants
                                    where !user.UserAppId.Equals(input.Comment.UserAppId)
                                    select new CommentInfo { CommentInfoId = 0, CommentId = 0, CreatedAt = nowUTC, Delivered = false, Seen = false, UserAppId = user.UserAppId };
                        // the current user must be Seen = True; SeenAt = NowUTC
                        CommentInfo ThisUser = new CommentInfo { CommentInfoId = 0, CommentId = 0, CreatedAt = nowUTC, Delivered = false, Seen = true, SeenAt = nowUTC, UserAppId = input.Comment.UserAppId };

                        input.Comment.CommentsInfo = new List<CommentInfo>();
                        input.Comment.CommentsInfo = query.ToList();
                        input.Comment.CommentsInfo.Add(ThisUser);

                        // Save the chat to the context
                        contextMGT.Comment.Add(input.Comment);

                        //***** 4. Save and Commit to the Database (Atomic because is same Context DB) 
                        if (!error && autoCommit)
                        {
                            // This commit returns the user from Database and updates input.Comment 
                            await contextMGT.SaveChangesAsync(); // Call it only once so do all other operations first
                        }

                        

                        // Save the notifications

                        String chatGroup = (chatDb.Participants.Count > 1) ? chatDb.Name : "";
                        
                        // Send to everyone except to himself
                        var queryNotification = from user in chatDb.Participants
                                                where user.UserAppId != userApp.UserAppId
                                                select new Notification
                                                {
                                                    NotificationId = 0,
                                                    Type = "NewComment",
                                                    Title = "*NewComment*",
                                                    Subtitle = chatGroup,
                                                    Body = chatGroup + " *has a new comment from* " + userApp.UserName,
                                                    Message = input.Comment.Message,                                                                                                        
                                                    ToUserAppId = user.UserAppId,
                                                    Route = "Comment",
                                                    RouteAction = "New",
                                                    RouteId = input.Comment.CommentId.ToString(),
                                                    CreatedAt = nowUTC,
                                                    Seen = false
                                                };

                        notifications = queryNotification.ToList();

                        // Save the Notifications to the context
                        contextMGT.Notification.AddRange(notifications);
                        
                        if (!error && autoCommit)
                        {
                            // This commit returns the user from Database and updates input.Comment 
                            await contextMGT.SaveChangesAsync(); // Call it only once so do all other operations first
                        }

                        //***** 5. Execute Send e-mails or other events once the database has been succesfully saved
                        //***** If this task fails, there are options -> 1. Retry multiple times 2. Save the event as Delay, 3.Rollback Database, Re

                        //***** 6. Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database
                        output.ResultConfirmation = ResultConfirmation.resultGood(_ResultMessage: "COMMENT_SUCESSFULLY_CREATED"); // If OK                        
                        output.Comment = input.Comment;                        
                    }// if (!error)

                    if (!error) // Send the notification to the subscriptors
                    {

                        // Create a Notification Queue

                        //var result = eventSender.SendAsync(new OnEventMessageDefault<Comment>(eventName: "onCommentAddedToChat", argumentTag: "chatId", argumentValue: output.Comment.ChatId.ToString(), outputType: output.Comment));
                        // Send the notification to each user * New Logic *

                        foreach (var notification in notifications)
                        {
                            var result = eventSender.SendAsync(new OnEventMessageDefault<Notification>(eventName: "onNotificationToUser", argumentTag: "userAppId", argumentValue: notification.ToUserAppId, outputType: notification));
                        }

                        /*
                        String chatGroup = (chatDb.Participants.Count > 1) ? " @ " + chatDb.Name : "";

                        int countDelivered = 0;
                        foreach (var user in chatDb.Participants)
                        {
                            String message = output.Comment.Message + " Mr. " + user.UserAppId;

                            message = userApp.UserName + " Says " + output.Comment.Message + chatGroup;
                            Notification notification = new Notification { NotificationId = 0, Created = nowUTC, Message = message };
                            var result = eventSender.SendAsync(new OnEventMessageDefault<Notification>(eventName: "onNotificationToUser", argumentTag: "userAppId", argumentValue: user.UserAppId, outputType: notification));

                            if (result.IsCompletedSuccessfully)
                            {
                                countDelivered++;
                            }


                        }
                        */



                        // 
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
                output = new AddCommentToChatTxn_Output(); // Restart variable to avoid returning any already saved data
                output.ResultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "EXCEPTION", _ResultDetail: ex.Message);
            }
            finally
            {
                // Save Logs if needed
            }

            return output;
        }
    }

}

