﻿using System;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Types;
using MGT_Exchange.GraphQLActions.Resources;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Models;
using MGT_Exchange.ChatAPI.GraphQL;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace MGT_Exchange.ChatAPI.Transactions
{

    // 1. Create Model: Input type is used for Mutation, it should be included if needed
    public class CreateChatTxn_Input
    {
        public Chat Chat { get; set; }
    }

    public class CreateChatTxn_InputType : InputObjectType<CreateChatTxn_Input>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateChatTxn_Input> descriptor)
        {
            descriptor.Field(t => t.Chat)
                .Type<ChatInputType>();
        }
    }

    // 2. Create Model: Output type is used for Mutation, it should be included if needed
    public class CreateChatTxn_Output
    {
        public ResultConfirmation ResultConfirmation { get; set; }
        public Chat Chat { get; set; }
    }

    public class CreateChatTxn_OutputType : ObjectType<CreateChatTxn_Output>
    {

    }

    // 4. Transaction - Logic Controller
    public class CreateChatTxn
    {

        public CreateChatTxn()
        {
        }

        public async Task<CreateChatTxn_Output> Execute(CreateChatTxn_Input input, MVCDbContext contextFather = null, bool autoCommit = true)
        {
            CreateChatTxn_Output output = new CreateChatTxn_Output();
            output.ResultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "TXN_NOT_STARTED");

            // Error handling
            bool error = false; // To Handle Only One Error
            bool chatFound = false; // To see if the chat already exists

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

                    // Company company = await contextMVC.Company.Where(x => x.Id.Equals(input.Company.Id)).FirstOrDefaultAsync();

                    // Validate user status, user company, user not blocked, etc

                    // One way to do it: Try to save without validations, if constraints fail (inner update db update) then run queries to check what went wrong

                    Company companyDb = await contextMGT.Company.Where(x => x.CompanyId.Equals(input.Chat.CompanyId)).FirstOrDefaultAsync();

                    if (companyDb == null)
                    {
                        error = true;
                        output.ResultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "COMPANY_DOES_NOT_EXIST_ERROR", _ResultDetail: "");
                    }

                    // Try to find a Chat already crated for the same users. If the chat exists, return the chat

                    if (!error)
                    {
                        DateTime nowUTC = DateTime.UtcNow;

                        //***** 0. Validate if the Chat exists, so return that Chat

                        List<String> userIds = input.Chat.Participants
                            .GroupBy(g => g.UserAppId)
                            .Select(user => user.Key) // extract unique Ids from users    
                            .ToList();

                        Chat chatDb = await contextMGT.Chat.Include(o => o.Participants)
                            .Where(company => company.CompanyId == input.Chat.CompanyId) // It's the same company
                            .Where(x => x.Participants.Count == input.Chat.Participants.Count) // Have the same number of users
                            .Where(x => x.Participants.All(users => userIds.Contains(users.UserAppId))) // Have the same users by Id
                            .FirstOrDefaultAsync();

                        if (chatDb != null)
                        {
                            chatFound = true;
                            
                            // Save the comments into the Database
                            foreach (var comment in input.Chat.Comments)
                            {
                                comment.CommentId = 0;
                                comment.ChatId = chatDb.ChatId;
                                comment.Created = nowUTC;
                                contextMGT.Comment.Add(comment);
                            }
                            output.ResultConfirmation = ResultConfirmation.resultGood(_ResultMessage: "CHAT_SUCESSFULLY_FOUND"); // If Found
                            output.Chat = chatDb;
                        }

                        //***** 1. Create the Chat (Atomic because is same Context DB)
                        if (!chatFound)
                        {
                            // Create the Chat To Save                            
                            input.Chat.ChatId = 0;
                            input.Chat.Created = nowUTC;
                            input.Chat.Comments.ForEach(x => x.Created = nowUTC);

                            // Save the chat to the context
                            contextMGT.Chat.Add(input.Chat);
                            output.ResultConfirmation = ResultConfirmation.resultGood(_ResultMessage: "CHAT_SUCESSFULLY_CREATED"); // If OK                        
                            output.Chat = input.Chat;
                        }

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
                output = new CreateChatTxn_Output(); // Restart variable to avoid returning any already saved data
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


/*****
    // 1. Create Model: Input type is used for Mutation, it should be included if needed
    public class CreateChatTxn_Input
    {
        public Chat Chat { get; set; }
    }

    public class CreateChatTxn_InputType : InputObjectType<CreateChatTxn_Input>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateChatTxn_Input> descriptor)
        {
            descriptor.Field(t => t.Chat)
                .Type<ChatInputType>();
        }
    }

    // 2. Create Model: Output type is used for Mutation, it should be included if needed
    public class CreateChatTxn_Output
    {
        public ResultConfirmation ResultConfirmation { get; set; }
        public Chat Chat { get; set; }
    }

    public class CreateChatTxn_OutputType : ObjectType<CreateChatTxn_Output>
    {

    }

    // 4. Transaction - Logic Controller
    public class CreateChatTxn
    {

        public CreateChatTxn()
        {
        }

        public async Task<CreateChatTxn_Output> Execute(CreateChatTxn_Input input, MVCDbContext contextFather = null, bool autoCommit = true)
        {
            CreateChatTxn_Output output = new CreateChatTxn_Output();
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
                output = new CreateChatTxn_Output(); // Restart variable to avoid returning any already saved data
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
