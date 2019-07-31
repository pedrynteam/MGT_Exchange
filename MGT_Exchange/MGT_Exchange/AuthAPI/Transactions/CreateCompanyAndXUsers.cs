using HotChocolate.Types;
using MGT_Exchange.AuthAPI.GraphQL;
using MGT_Exchange.AuthAPI.MVC;
using MGT_Exchange.ChatAPI.GraphQL;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Data;
using MGT_Exchange.GraphQLActions;
using MGT_Exchange.GraphQLActions.Resources;
using MGT_Exchange.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.AuthAPI.Transactions
{

    // 1. Create Model: Input type is used for Mutation, it should be included if needed
    public class CreateCompanyAndXUsersTxn_Input
    {        
        public company Company { get; set; }
        public int UsersToCreate { get; set; }
    }

    public class CreateCompanyAndXUsersTxn_InputType : InputObjectType<CreateCompanyAndXUsersTxn_Input>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateCompanyAndXUsersTxn_Input> descriptor)
        {
            descriptor.Field(t => t.Company)
                .Type<CompanyInputType>();
            descriptor.Field(t => t.UsersToCreate)
                .Type<IntType>();
        }
    }

    // 2. Create Model: Output type is used for Mutation, it should be included if needed
    public class CreateCompanyAndXUsersTxn_Output
    {
        public resultConfirmation ResultConfirmation { get; set; }
        public company Company { get; set; }
    }

    public class CreateCompanyAndXUsersTxn_OutputType : ObjectType<CreateCompanyAndXUsersTxn_Output>
    {

    }

    // 4. Transaction - Logic Controller
    public class CreateCompanyAndXUsersTxn
    {

        public CreateCompanyAndXUsersTxn()
        {
        }

        public async Task<CreateCompanyAndXUsersTxn_Output> Execute(CreateCompanyAndXUsersTxn_Input input, IServiceProvider serviceProvider, MVCDbContext contextFatherMVC = null, bool autoCommit = true)
        {
            CreateCompanyAndXUsersTxn_Output output = new CreateCompanyAndXUsersTxn_Output();
            output.ResultConfirmation = resultConfirmation.resultBad(_ResultMessage: "TXN_NOT_STARTED");
            //String userType = "User";

            // Error handling
            bool error = false; // To Handle Only One Error

            try
            {                
                MVCDbContext contextMVC = (contextFatherMVC != null) ? contextFatherMVC : new MVCDbContext();
                // An using statement is in reality a try -> finally statement, disposing the element in the finally. So we need to take advance of that to create a DBContext inheritance                
                try
                {
                    // DBContext by convention is a UnitOfWork, track changes and commits when SaveChanges is called
                    // Multithreading issue: so if we use only one DBContext, it will track all entities (_context.Customer.Add) and commit them when SaveChanges is called, 
                    // No Matter what transactions or client calls SaveChanges.
                    // Note: Rollback will not remove the entities from the tracker in the context. so better dispose it.

                    //***** 0. Make The Validations - Be careful : Concurrency. Same name can be saved multiple times if called at the exact same time. Better have an alternate database constraint

                    // Call the Transaction to Create the Company
                    
                    //CreateCompanyTxn_Input CreateCompanyTxn_Input = new CreateCompanyTxn_Input { Company = input.Company };
                    //CreateCompanyTxn_Output CreateCompanyTxn = await new GraphQLMutation().CreateCompanyTxn(input: CreateCompanyTxn_Input, serviceProvider: serviceProvider);

                    CreateCompanyTxn_Input CreateCompanyTxn_Input = new CreateCompanyTxn_Input { Company = input.Company};
                    CreateCompanyTxn_Output CreateCompanyTxn = await new CreateCompanyTxn().Execute(input: CreateCompanyTxn_Input, serviceProvider: serviceProvider);

                    if (!CreateCompanyTxn.ResultConfirmation.resultPassed)
                    {
                        // Return the error as it is from the transaction
                        error = true;
                        output.ResultConfirmation = CreateCompanyTxn.ResultConfirmation;
                        return output;
                    }

                    output.Company = CreateCompanyTxn.Company;

                    // Now create a loop to create the X users. Use the same database
                    for (int x = 1; x <= input.UsersToCreate; x++)
                    {
                        userApp newUser = new userApp { firstName = "Name"+x, lastName = "LastName"+x, nickname = "Nickname"+x, userName = CreateCompanyTxn.Company.companyId+ "--" + x.ToString() };
                        
                        CreateUserTxn_Input CreateUserTxn_Input = new CreateUserTxn_Input { Company = CreateCompanyTxn.Company, User = newUser };
                        CreateUserTxn_Output CreateUserTxn = await new CreateUserTxn().Execute(input: CreateUserTxn_Input, serviceProvider: serviceProvider, contextFatherMVC: contextMVC);
                        
                        if (!CreateUserTxn.ResultConfirmation.resultPassed)
                        {
                            // Return the error as it is from the transaction
                            error = true;
                            output.ResultConfirmation = CreateUserTxn.ResultConfirmation;
                            return output;
                        }
                    }

                    
                    // Logic, first create All the users in MVC Database. Commit, if something goes wrong return Error, if OK create users in master user App table.

                    // 
                    // UserManager<IdentityUser> _userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

                    /*
                    UserManager<IdentityUser> _userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

                    var user = new IdentityUser { UserName = input.user.UserName, Email = input.user.Email, PasswordHash = input.user.PasswordHash };
                    var result = await _userManager.CreateAsync(user);

                    if (!result.Succeeded)
                    {
                        error = true;
                        _output.ResultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "USER_NOT_CREATED_ERROR", _ResultDetail: result.Errors.FirstOrDefault().Description); // If OK  
                        List<ItemKey> resultKeys = new List<ItemKey>();

                        foreach (var errorDesc in result.Errors)
                        {
                            resultKeys.Add(new ItemKey(errorDesc.Code, errorDesc.Description));
                        }

                        _output.ResultConfirmation.ResultDictionary = resultKeys;

                    }
                    */

                    if (!error)
                    {
                        //***** 1. Create the token (Atomic because is same Context DB)                        

                        /*
                        // Save the Name Claim into the database
                        var _claims = new[]{
                            new Claim(ClaimTypes.Name, user.Id)
                            ,new Claim("UserType", userType) // Create the token without Manager status, then add the claim in database
                        };

                        var resultClaims = await _userManager.AddClaimsAsync(user, _claims); // Save claims in the Database
                        


                        // Call transaction to get a new Token (we call the Mutation not the class itself) 
                        var tokenString = "";
                        CreateTokenTxn_Input createTokenTxn_Input = new CreateTokenTxn_Input { userAppId = user.Id };
                        CreateTokenTxn_Output createTokenTxn = await new GraphQLMutation().CreateTokenTxn(input: createTokenTxn_Input, serviceProvider: serviceProvider);
                        if (createTokenTxn.resultConfirmation.ResultPassed)
                        {
                            tokenString = createTokenTxn.token;
                        }
                        */


                        //***** 4. Save and Commit to the Database (Atomic because is same Context DB) 
                        if (!error && autoCommit)
                        {
                            await contextMVC.SaveChangesAsync(); // Call it only once so do all other operations first                            
                        }

                        //***** 5. Execute Send e-mails or other events once the database has been succesfully saved
                        //***** If this task fails, there are options -> 1. Retry multiple times 2. Save the event as Delay, 3.Rollback Database, Re

                        //***** 6. Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database
                        output.ResultConfirmation = resultConfirmation.resultGood(_ResultMessage: "COMPANY_SUCESSFULLY_CREATED"); // If OK
                        //_output.token = tokenString; // The token                        
                        //_output.user = input.user;
                    }// if (!error)
                }
                finally
                {
                    // If the context Father is null the context was created on his own, so dispose it
                    if (contextMVC != null && contextFatherMVC == null)
                    {
                        contextMVC.Dispose();
                    }
                }
            }
            catch (Exception ex) // Main try 
            {
                System.Diagnostics.Debug.WriteLine("Error: " + ex.Message);
                string innerError = (ex.InnerException != null) ? ex.InnerException.Message : "";
                System.Diagnostics.Debug.WriteLine("Error Inner: " + innerError);
                output = new CreateCompanyAndXUsersTxn_Output(); // Restart variable to avoid returning any already saved data
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
