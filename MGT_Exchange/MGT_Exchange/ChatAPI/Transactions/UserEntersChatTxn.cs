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
using MGT_Exchange.AuthAPI.Transactions;
using MGT_Exchange.AuthAPI.GraphQL;

namespace MGT_Exchange.ChatAPI.Transactions
{
    // 1. Create Model: Input type is used for Mutation, it should be included if needed
    public class UserEntersChatTxn_Input
    {
        // Use the Model just to avoid changes needed in the future
        public chat Chat { get; set; }
        public userApp User { get; set; }
    }

    public class UserEntersChatTxn_InputType : InputObjectType<UserEntersChatTxn_Input>
    {
        protected override void Configure(IInputObjectTypeDescriptor<UserEntersChatTxn_Input> descriptor)
        {
            descriptor.Field(t => t.Chat)
                .Type<NonNullType<ChatInputType>>();
            descriptor.Field(t => t.User)
                .Type<NonNullType<UserAppInputType>>();
        }
    }

    // 2. Create Model: Output type is used for Mutation, it should be included if needed
    public class UserEntersChatTxn_Output
    {
        public resultConfirmation ResultConfirmation { get; set; }
        public chat Chat { get; set; }
    }

    public class UserEntersChatTxn_OutputType : ObjectType<UserEntersChatTxn_Output>
    {

    }

    // 4. Transaction - Logic Controller
    public class UserEntersChatTxn
    {

        public UserEntersChatTxn()
        {
        }

        public async Task<UserEntersChatTxn_Output> Execute(UserEntersChatTxn_Input input, MVCDbContext contextFather = null, bool autoCommit = true, IEventSender eventSender = null)
        {
            UserEntersChatTxn_Output output = new UserEntersChatTxn_Output();
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

                    // Find all the messaget that this user has not seen in this Chat



                    // Chat exists and the user belong to this Chat
                    chat chatDb = await contextMGT.Chat.Include(o => o.participants)
                        .Where(x => x.chatId == input.Chat.chatId)
                        .Where(x => x.participants.Any(y => y.userAppId.Equals(input.User.userAppId)))
                        .FirstOrDefaultAsync();
                    ;

                    if (chatDb == null)
                    {
                        error = true;
                        output.ResultConfirmation = resultConfirmation.resultBad(_ResultMessage: "USER_NOT_AUTHORIZED");
                    }

                    /* Find the pending comments to be read for the user. There are two types
                     * 1. CommentInfo that dont exist (there is no row). No even delivery
                     * 1. CommentInfo that exist and Seen is false and SeenAt is Empty (there is no row). No even delivery
                     */



                    
                    if (!error)
                    {
                        //***** 1. Create the Comment (Atomic because is same Context DB)

                        // Create the Chat To Save
                        // input.Comment.CommentId = 0;
                        // input.Comment.Created = DateTime.UtcNow;

                        // Save the chat to the context
                        DateTime nowUTC = DateTime.UtcNow;

                        List<comment> commentsToReadNoExist = await contextMGT.Comment.Include(o => o.commentsInfo)    
                            .Where(x => x.chatId == input.Chat.chatId)    
                            .Where(x => !x.commentsInfo.Any(y => y.userAppId.Equals(input.User.userAppId)))    
                            .ToListAsync();
                        ;

                        foreach (var comment in commentsToReadNoExist)
                        {
                            commentInfo commentInfo = new commentInfo { commentInfoId = 0, commentId = comment.commentId, delivered = true, deliveredAt = nowUTC, seen = true, seenAt = nowUTC, userAppId = input.User.userAppId };
                            contextMGT.CommentInfo.Add(commentInfo);
                        }
                         
                        // contextMGT.Comment.Add(input.Comment);

                        //***** 4. Save and Commit to the Database (Atomic because is same Context DB) 
                        if (!error && autoCommit)
                        {
                            // This commit returns the user from Database and updates input.Comment 
                            await contextMGT.SaveChangesAsync(); // Call it only once so do all other operations first
                        }

                        //***** 5. Execute Send e-mails or other events once the database has been succesfully saved
                        //***** If this task fails, there are options -> 1. Retry multiple times 2. Save the event as Delay, 3.Rollback Database, Re

                        //***** 6. Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database
                        output.ResultConfirmation = resultConfirmation.resultGood(_ResultMessage: "CHAT_SUCESSFULLY_FOUND"); // If OK                        
                        chatDb.comments = new List<comment>();
                        chatDb.comments = commentsToReadNoExist;
                        output.Chat = chatDb;
                    }// if (!error)

                    if (!error) // Send the notification to the subscriptors
                    {
                        // await eventSender.SendAsync(new OnEventMessageDefault<Comment>(eventName: "onCommentAddedToChat", argumentTag: "chatId", argumentValue: output.Comment.ChatId.ToString(), outputType: output.Comment));
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
                output = new UserEntersChatTxn_Output(); // Restart variable to avoid returning any already saved data
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


/*****
    // 1. Create Model: Input type is used for Mutation, it should be included if needed
    public class UserEntersChatTxn_Input
    {
        public Chat Chat { get; set; }
    }

    public class UserEntersChatTxn_InputType : InputObjectType<UserEntersChatTxn_Input>
    {
        protected override void Configure(IInputObjectTypeDescriptor<UserEntersChatTxn_Input> descriptor)
        {
            descriptor.Field(t => t.Chat)
                .Type<ChatInputType>();
        }
    }

    // 2. Create Model: Output type is used for Mutation, it should be included if needed
    public class UserEntersChatTxn_Output
    {
        public ResultConfirmation ResultConfirmation { get; set; }
        public Chat Chat { get; set; }
    }

    public class UserEntersChatTxn_OutputType : ObjectType<UserEntersChatTxn_Output>
    {

    }

    // 4. Transaction - Logic Controller
    public class UserEntersChatTxn
    {

        public UserEntersChatTxn()
        {
        }

        public async Task<UserEntersChatTxn_Output> Execute(UserEntersChatTxn_Input input, MVCDbContext contextFather = null, bool autoCommit = true)
        {
            UserEntersChatTxn_Output output = new UserEntersChatTxn_Output();
            output.ResultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "TXN_NOT_STARTED");

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

                    
                    /*
                    if (!result.Succeeded)
                    {
                        error = true;
                        _output.resultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "Chat_NOT_CREATED_ERROR", _ResultDetail: result.Errors.FirstOrDefault().Description); // If OK  
                        List<ItemKey> resultKeys = new List<ItemKey>();

                        foreach (var errorDesc in result.Errors)
                        {
                            resultKeys.Add(new ItemKey(errorDesc.Code, errorDesc.Description));
                        }
                        _output.resultConfirmation.ResultDictionary = resultKeys;
                    }
                    */
/****
                    if (!error)
                    {
                        //***** 1. Create the token (Atomic because is same Context DB)

                        input.Chat.Created = DateTime.Now;                        

                        contextMGT.Chat.Add(input.Chat);

                        //***** 4. Save and Commit to the Database (Atomic because is same Context DB) 
                        if (!error && autoCommit)
                        {
                            await contextMGT.SaveChangesAsync(); // Call it only once so do all other operations first
                        }

                        //***** 5. Execute Send e-mails or other events once the database has been succesfully saved
                        //***** If this task fails, there are options -> 1. Retry multiple times 2. Save the event as Delay, 3.Rollback Database, Re

                        //***** 6. Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database
                        output.ResultConfirmation = ResultConfirmation.resultGood(_ResultMessage: "CHAT_SUCESSFULLY_CREATED"); // If OK                        
                        output.Chat = input.Chat;
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
                output = new UserEntersChatTxn_Output(); // Restart variable to avoid returning any already saved data
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
*/

