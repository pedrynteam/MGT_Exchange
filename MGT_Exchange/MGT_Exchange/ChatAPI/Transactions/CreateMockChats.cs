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
using MGT_Exchange.AuthAPI.Transactions;
using MGT_Exchange.GraphQLActions;
using MGT_Exchange.AuthAPI.MVC;

namespace MGT_Exchange.ChatAPI.Transactions
{

    // 1. Create Model: Input type is used for Mutation, it should be included if needed
    public class CreateMockChatsTxn_Input
    {
        public string CompanyName { get; set; }
        public int UsersToCreate { get; set; }
        public int ChatsToCreate { get; set; }
    }

    public class CreateMockChatsTxn_InputType : InputObjectType<CreateMockChatsTxn_Input>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateMockChatsTxn_Input> descriptor)
        {
            

        }
    }

    // 2. Create Model: Output type is used for Mutation, it should be included if needed
    public class CreateMockChatsTxn_Output
    {
        public ResultConfirmation ResultConfirmation { get; set; }
        public List<Chat> Chats { get; set; }
    }

    public class CreateMockChatsTxn_OutputType : ObjectType<CreateMockChatsTxn_Output>
    {

    }

    // 4. Transaction - Logic Controller
    public class CreateMockChatsTxn
    {

        public CreateMockChatsTxn()
        {
        }

        public async Task<CreateMockChatsTxn_Output> Execute(CreateMockChatsTxn_Input input, IServiceProvider serviceProvider, MVCDbContext contextFather = null, bool autoCommit = true)
        {
            CreateMockChatsTxn_Output output = new CreateMockChatsTxn_Output();
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

                    // Company company = await contextMVC.Company.Where(x => x.Id.Equals(input.Company.Id)).FirstOrDefaultAsync();

                    // Validate user status, user company, user not blocked, etc

                    // One way to do it: Try to save without validations, if constraints fail (inner update db update) then run queries to check what went wrong

                    // Call transaction to create a New Company
                    CreateCompanyTxn_Input createCompanyTxn_Input = new CreateCompanyTxn_Input { Company = new Company { Name = input.CompanyName, Email = input.CompanyName + "@company.com", Password = input.CompanyName + "123" } };
                    CreateCompanyTxn_Output createCompanyTxn = await new GraphQLMutation().CreateCompanyTxn(input: createCompanyTxn_Input, serviceProvider: serviceProvider);
                    if (!createCompanyTxn.ResultConfirmation.ResultPassed)
                    {
                        error = true;
                        output.ResultConfirmation = createCompanyTxn.ResultConfirmation;
                        return output;
                    }

                    // Create Users
                    for (int i = 0; i < input.UsersToCreate; i++)
                    {
                        // Call transaction to create a New User
                        CreateUserTxn_Input createUserTxn_Input = new CreateUserTxn_Input { User = new UserApp { UserName = i.ToString()+"_"+ createCompanyTxn.Company.CompanyId, Email = i.ToString() + "@User.com", Password = "1234567" }, Company = new Company { CompanyId = createCompanyTxn.Company.CompanyId } };
                        CreateUserTxn_Output createUserTxn = await new GraphQLMutation().CreateUserTxn(input: createUserTxn_Input, serviceProvider: serviceProvider);
                        if (!createUserTxn.ResultConfirmation.ResultPassed)
                        {
                            error = true;
                            output.ResultConfirmation = createUserTxn.ResultConfirmation;
                            return output;
                        }
                    }

                    // Create chats
                    List<Chat> chatsNew = new List<Chat>();
                    for (int i = 0; i < input.ChatsToCreate; i++)
                    {
                        DateTime utcNow = DateTime.UtcNow;
                        // Create the chat
                        Chat chat = new Chat { ChatId = 0, Name = "Chat " + i, CompanyId = createCompanyTxn.Company.CompanyId, CreatedAt = utcNow, UpdatedAt = utcNow, Type = "Chat" };

                        // Add participants
                        List<Participant> participants = (from user in contextMGT.UserApp
                                                          where user.CompanyId.Equals(createCompanyTxn.Company.CompanyId)
                                                         select new Participant { ParticipantId = 0, ChatId = 0, IsAdmin = false, UserAppId = user.UserAppId })
                                                         .OrderBy(x => x.UserAppId)
                                                         .Take(input.UsersToCreate-i)
                                                         .ToList();

                        chat.Participants = participants;

                        /*
                        // Add comments Seen
                        List<Comment> commentsSeen = (from part in participants
                                                     select new Comment { CommentId = 0, ChatId = 0, Message = "Message ", CreatedAt = utcNow.AddMinutes(-1), UserAppId = part.UserAppId })
                                                     .ToList();

                        foreach (var comm in commentsSeen)
                        {
                            // Create and Save the CommentInfo for each user. 
                            var query = from user in participants
                                        select new CommentInfo { CommentInfoId = 0, CommentId = 0, CreatedAt = utcNow.AddMinutes(-1), Delivered = true, Seen = true, SeenAt = utcNow.AddMinutes(-1), UserAppId = user.UserAppId };

                            comm.CommentsInfo = query.ToList();
                        }

                        // Add comments Seen
                        List<Comment> commentsUnseen = (from part in participants
                                                      select new Comment { CommentId = 0, ChatId = 0, Message = "Message ", CreatedAt = utcNow, UserAppId = part.UserAppId })
                                                     .ToList();

                        foreach (var comm in commentsUnseen)
                        {
                            // Create and Save the CommentInfo for each user. 
                            var query = from user in participants
                                        select new CommentInfo { CommentInfoId = 0, CommentId = 0, CreatedAt = utcNow, Delivered = true, Seen = false, UserAppId = user.UserAppId };

                            comm.CommentsInfo = query.ToList();
                        }
                        */

                        // Add one comment for each participant 
                        List<Comment> commentsUnseen = (from part in participants
                                                        select new Comment { CommentId = 0, ChatId = 0, Message = "Msg "+part.UserAppId, CreatedAt = utcNow, UserAppId = part.UserAppId })
                                                     .ToList();

                        foreach (var comm in commentsUnseen)
                        {
                            // Create and Save the CommentInfo for each user. 
                            var query = from user in participants
                                        select new CommentInfo { CommentInfoId = 0, CommentId = 0, CreatedAt = utcNow, Delivered = true, Seen = false, UserAppId = user.UserAppId };

                            comm.CommentsInfo = query.ToList();
                        }

                        chat.Comments = commentsUnseen;

                        // Call transaction to create a New Chat
                        CreateChatTxn_Input createChatTxn_Input = new CreateChatTxn_Input { Chat = chat  };
                        CreateChatTxn_Output createChatTxn = await new GraphQLMutation().CreateChatTxn(input: createChatTxn_Input);
                        if (!createChatTxn.ResultConfirmation.ResultPassed)
                        {
                            error = true;
                            output.ResultConfirmation = createChatTxn.ResultConfirmation;
                            return output;
                        }

                        chatsNew.Add(createChatTxn.Chat);

                        

                        //***** 4. Save and Commit to the Database (Atomic because is same Context DB) 
                        if (!error && autoCommit)
                        {
                            await contextMGT.SaveChangesAsync(); // Call it only once so do all other operations first
                        }

                    }// for (int i = 0; i < input.ChatsToCreate; i++)

                    //***** 4. Save and Commit to the Database (Atomic because is same Context DB) 
                    if (!error && autoCommit)
                    {
                        await contextMGT.SaveChangesAsync(); // Call it only once so do all other operations first
                    }

                    //***** 5. Execute Send e-mails or other events once the database has been succesfully saved
                    //***** If this task fails, there are options -> 1. Retry multiple times 2. Save the event as Delay, 3.Rollback Database, Re

                    //***** 6. Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database                        
                    output.ResultConfirmation = ResultConfirmation.resultGood(_ResultMessage: "CHAT_SUCESSFULLY_CREATED"); // If OK                        
                    output.Chats = chatsNew;
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
                output = new CreateMockChatsTxn_Output(); // Restart variable to avoid returning any already saved data
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
