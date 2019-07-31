using HotChocolate.Types;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.Extensions.DependencyInjection;
using MGT_Exchange.GraphQLActions.Resources;
using MGT_Exchange.Data;
using MGT_Exchange.GraphQLActions;
using MGT_Exchange.AuthAPI.GraphQL;

namespace MGT_Exchange.AuthAPI.Transactions
{
    // 1. Create Model: Input type is used for Mutation, it should be included if needed
    public class CreateIdentityUserTxn_Input
    {
        public IdentityUser IdentityUser { get; set; }
    }

    public class CreateIdentityUserTxn_InputType : InputObjectType<CreateIdentityUserTxn_Input>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateIdentityUserTxn_Input> descriptor)
        {
            descriptor.Field(t => t.IdentityUser)
                .Type<IdentityUserInputType>();
        }
    }

    // 2. Create Model: Output type is used for Mutation, it should be included if needed
    public class CreateIdentityUserTxn_Output
    {
        public resultConfirmation ResultConfirmation { get; set; }
        public IdentityUser IdentityUser { get; set; }
        public String Token { get; set; } // This will contain the token created if everything goes ok        
    }

    public class CreateIdentityUserTxn_OutputType : ObjectType<CreateIdentityUserTxn_Output>
    {

    }

    // 4. Transaction - Logic Controller
    public class CreateIdentityUserTxn
    {

        public CreateIdentityUserTxn()
        {
        }

        public async Task<CreateIdentityUserTxn_Output> Execute(CreateIdentityUserTxn_Input input, IServiceProvider serviceProvider, ApplicationDbContext contextFather = null, bool autoCommit = true)
        {
            CreateIdentityUserTxn_Output _output = new CreateIdentityUserTxn_Output();
            _output.ResultConfirmation = resultConfirmation.resultBad(_ResultMessage: "TXN_NOT_STARTED");

            // Error handling
            bool error = false; // To Handle Only One Error

            try
            {
                ApplicationDbContext contextMGT = (contextFather != null) ? contextFather : new ApplicationDbContext();
                // An using statement is in reality a try -> finally statement, disposing the element in the finally. So we need to take advance of that to create a DBContext inheritance                
                try
                {
                    // DBContext by convention is a UnitOfWork, track changes and commits when SaveChanges is called
                    // Multithreading issue: so if we use only one DBContext, it will track all entities (_context.Customer.Add) and commit them when SaveChanges is called, 
                    // No Matter what transactions or client calls SaveChanges.
                    // Note: Rollback will not remove the entities from the tracker in the context. so better dispose it.

                    //***** 0. Make The Validations - Be careful : Concurrency. Same name can be saved multiple times if called at the exact same time. Better have an alternate database constraint

                    UserManager<IdentityUser> _userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

                    var user = new IdentityUser { UserName = input.IdentityUser.UserName, Email = input.IdentityUser.Email, PasswordHash = input.IdentityUser.PasswordHash };
                    var result = await _userManager.CreateAsync(user);

                    if (!result.Succeeded)
                    {
                        error = true;
                        _output.ResultConfirmation = resultConfirmation.resultBad(_ResultMessage: "USER_NOT_CREATED_ERROR", _ResultDetail: result.Errors.FirstOrDefault().Description); // If OK  
                        List<itemKey> resultKeys = new List<itemKey>();

                        foreach (var errorDesc in result.Errors)
                        {
                            resultKeys.Add(new itemKey(errorDesc.Code, errorDesc.Description));
                        }

                        _output.ResultConfirmation.resultDictionary = resultKeys;

                    }

                    if (!error)
                    {
                        //***** 1. Create the token (Atomic because is same Context DB)

                        // Save the Name Claim into the database
                        var _claims = new[]{
                            new Claim(ClaimTypes.Name, user.Id),
                            // ,new Claim("EmployeeStatus", "Manager") Create the token without Manager status, then add the claim in database
                        };

                        var resultClaims = await _userManager.AddClaimsAsync(user, _claims); // Save claims in the Database

                        // Call transaction to get a new Token (we call the Mutation not the class itself) 
                        var tokenString = "";
                        CreateTokenTxn_Input createTokenTxn_Input = new CreateTokenTxn_Input { userAppId = user.Id };
                        CreateTokenTxn_Output createTokenTxn = await new GraphQLMutation().CreateTokenTxn(input: createTokenTxn_Input, serviceProvider: serviceProvider);
                        if (createTokenTxn.resultConfirmation.resultPassed)
                        {
                            tokenString = createTokenTxn.token;
                        }

                        //***** 4. Save and Commit to the Database (Atomic because is same Context DB) 
                        if (!error && autoCommit)
                        {
                            await contextMGT.SaveChangesAsync(); // Call it only once so do all other operations first
                        }

                        //***** 5. Execute Send e-mails or other events once the database has been succesfully saved
                        //***** If this task fails, there are options -> 1. Retry multiple times 2. Save the event as Delay, 3.Rollback Database, Re

                        //***** 6. Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database
                        _output.ResultConfirmation = resultConfirmation.resultGood(_ResultMessage: "USER_SUCESSFULLY_CREATED"); // If OK
                        _output.Token = tokenString; // The token
                        input.IdentityUser.PasswordHash = ""; // Dont return the password
                        input.IdentityUser.Id = user.Id;
                        _output.IdentityUser = input.IdentityUser;
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
                _output = new CreateIdentityUserTxn_Output(); // Restart variable to avoid returning any already saved data
                _output.ResultConfirmation = resultConfirmation.resultBad(_ResultMessage: "EXCEPTION", _ResultDetail: ex.Message);
            }
            finally
            {
                // Save Logs if needed
            }

            return _output;
        }

    }




}
