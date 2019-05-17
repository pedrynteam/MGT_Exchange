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

namespace MGT_Exchange.ChatAPI.Transactions
{

    // 1. Create Model: Input type is used for Mutation, it should be included if needed
    public class GetCommentsFromChatTxn_Input
    {
        public Chat Chat { get; set; }
    }

    public class GetCommentsFromChatTxn_InputType : InputObjectType<GetCommentsFromChatTxn_Input>
    {
        protected override void Configure(IInputObjectTypeDescriptor<GetCommentsFromChatTxn_Input> descriptor)
        {
            descriptor.Field(t => t.Chat)
                .Type<ChatInputType>();
        }
    }

    // 2. Create Model: Output type is used for Mutation, it should be included if needed
    public class GetCommentsFromChatTxn_Output
    {
        public ResultConfirmation ResultConfirmation { get; set; }
        public Chat Chat { get; set; }
    }

    public class GetCommentsFromChatTxn_OutputType : ObjectType<GetCommentsFromChatTxn_Output>
    {

    }

    // 4. Transaction - Logic Controller
    public class GetCommentsFromChatTxn
    {

        public GetCommentsFromChatTxn()
        {
        }

        public async Task<GetCommentsFromChatTxn_Output> Execute(GetCommentsFromChatTxn_Input input, MVCDbContext contextFather = null, bool autoCommit = true)
        {
            GetCommentsFromChatTxn_Output output = new GetCommentsFromChatTxn_Output();
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

                    // Call Database to validate that: 1. ChatId exists 2. User Exists 3. User is part of the Company 4. User is Participant of the Chat

                    Chat chatDb = await contextMGT.Chat.Include(o => o.Participants)
                        .Where(x => x.ChatId == input.Chat.ChatId)
                        .FirstOrDefaultAsync();
                    
                    if (chatDb == null)
                    {
                        error = true;
                        output.ResultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "CHAT_DOES_NOT_EXIST");
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
                output = new GetCommentsFromChatTxn_Output(); // Restart variable to avoid returning any already saved data
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

