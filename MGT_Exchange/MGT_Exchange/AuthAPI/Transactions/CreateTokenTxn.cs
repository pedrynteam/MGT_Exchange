using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HotChocolate.Types;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MGT_Exchange.GraphQLActions.Resources;
using MGT_Exchange.Data;

namespace MGT_Exchange.AuthAPI.Transactions
{
    // 1. Create Model: Input type is used for Mutation, it should be included if needed
    public class CreateTokenTxn_Input
    {
        public String userAppId { get; set; }
    }

    public class CreateTokenTxn_InputType : InputObjectType<CreateTokenTxn_Input>
    {

    }

    // 2. Create Model: Output type is used for Mutation, it should be included if needed
    public class CreateTokenTxn_Output
    {
        public ResultConfirmation resultConfirmation { get; set; }
        public String token { get; set; } // This will contain the token created if everything goes ok
    }

    public class CreateTokenTxn_OutputType : ObjectType<CreateTokenTxn_Output>
    {

    }

    // 4. Transaction - Logic Controller
    public class CreateTokenTxn
    {
        public CreateTokenTxn()
        {
        }

        public async Task<CreateTokenTxn_Output> Execute(CreateTokenTxn_Input input, IServiceProvider serviceProvider, ApplicationDbContext contextFather = null, bool autoCommit = true)
        {
            CreateTokenTxn_Output _output = new CreateTokenTxn_Output();
            _output.resultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "TXN_NOT_STARTED");

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

                    UserManager<IdentityUser> userManager = serviceProvider.GetService<UserManager<IdentityUser>>();

                    var user = await userManager.FindByIdAsync(input.userAppId);

                    if (user == null)
                    {
                        error = true;
                        _output.resultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "USER_NOT_FOUND_ERROR", _ResultDetail: input.userAppId); // If Error
                    }

                    if (!error)
                    {
                        //***** 1. Create the token (Atomic because is same Context DB)

                        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345"));
                        var signinCredentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);
                        string _issuer = "https://localhost:44359";
                        string _audience = "https://localhost:44359";

                        // Get the claim from the Database and group to avoid duplicates
                        var claimsDb = await (from claim in contextMGT.UserClaims
                                              where claim.UserId.Equals(user.Id)
                                              group claim by new { claim.ClaimType, claim.ClaimValue } into g
                                              select new Claim(g.Key.ClaimType, g.Key.ClaimValue)
                                ).ToListAsync()
                                ;

                        var tokenOptions = new JwtSecurityToken(
                            issuer: _issuer,
                            audience: _audience,
                            claims: claimsDb,
                            expires: DateTime.Now.AddDays(2),
                            signingCredentials: signinCredentials
                        );

                        var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

                        System.Diagnostics.Debug.WriteLine("Token: " + tokenString);

                        //***** 4. Save and Commit to the Database (Atomic because is same Context DB) 
                        if (!error && autoCommit)
                        {
                            await contextMGT.SaveChangesAsync(); // Call it only once so do all other operations first
                        }

                        //***** 5. Execute Send e-mails or other events once the database has been succesfully saved
                        //***** If this task fails, there are options -> 1. Retry multiple times 2. Save the event as Delay, 3.Rollback Database, Re

                        //***** 6. Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database
                        _output.resultConfirmation = ResultConfirmation.resultGood(_ResultMessage: "TOKEN_SUCESSFULLY_CREATED"); // If OK
                        _output.token = tokenString; // The token
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
                _output = new CreateTokenTxn_Output(); // Restart variable to avoid returning any already saved data
                _output.resultConfirmation = ResultConfirmation.resultBad(_ResultMessage: "EXCEPTION", _ResultDetail: ex.Message);
            }
            finally
            {
                // Save Logs if needed
            }

            return _output;
        }

    }




}
