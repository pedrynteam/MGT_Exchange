﻿using HotChocolate.Types;
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
    public class CreateUsersForCompanyTxn_Input
    {
        public List<userApp> Users { get; set; }
        public company Company { get; set; }
    }

    public class CreateUsersForCompanyTxn_InputType : InputObjectType<CreateUsersForCompanyTxn_Input>
    {
        protected override void Configure(IInputObjectTypeDescriptor<CreateUsersForCompanyTxn_Input> descriptor)
        {
            descriptor.Field(t => t.Company)
                .Type<CompanyInputType>();
            descriptor.Field(t => t.Users)
                .Type<ListType<UserAppType>>();
        }
    }

    // 2. Create Model: Output type is used for Mutation, it should be included if needed
    public class CreateUsersForCompanyTxn_Output
    {
        public resultConfirmation ResultConfirmation { get; set; }
        public company Company { get; set; }
    }

    public class CreateUsersForCompanyTxn_OutputType : ObjectType<CreateUsersForCompanyTxn_Output>
    {

    }

    // 4. Transaction - Logic Controller
    public class CreateUsersForCompanyTxn
    {

        public CreateUsersForCompanyTxn()
        {
        }

        public async Task<CreateUsersForCompanyTxn_Output> Execute(CreateUsersForCompanyTxn_Input input, IServiceProvider serviceProvider, MVCDbContext contextFatherMVC = null, ApplicationDbContext contextFatherApp = null, bool autoCommit = true)
        {
            CreateUsersForCompanyTxn_Output _output = new CreateUsersForCompanyTxn_Output();
            _output.ResultConfirmation = resultConfirmation.resultBad(_ResultMessage: "TXN_NOT_STARTED");
            //String userType = "User";

            // Error handling
            bool error = false; // To Handle Only One Error

            try
            {
                ApplicationDbContext contextApp = (contextFatherApp != null) ? contextFatherApp : new ApplicationDbContext();
                MVCDbContext contextMVC = (contextFatherMVC != null) ? contextFatherMVC : new MVCDbContext();
                // An using statement is in reality a try -> finally statement, disposing the element in the finally. So we need to take advance of that to create a DBContext inheritance                
                try
                {
                    // DBContext by convention is a UnitOfWork, track changes and commits when SaveChanges is called
                    // Multithreading issue: so if we use only one DBContext, it will track all entities (_context.Customer.Add) and commit them when SaveChanges is called, 
                    // No Matter what transactions or client calls SaveChanges.
                    // Note: Rollback will not remove the entities from the tracker in the context. so better dispose it.

                    //***** 0. Make The Validations - Be careful : Concurrency. Same name can be saved multiple times if called at the exact same time. Better have an alternate database constraint

                    //Company company = await contextMVC.Company.Where(x => x.Id.Equals(input.Company.Id)).FirstOrDefaultAsync();
                    company company = await contextMVC.Company.Where(x => x.companyId.Equals(input.Company.companyId)).FirstOrDefaultAsync();

                    if (company == null)
                    {
                        error = true;
                        _output.ResultConfirmation = resultConfirmation.resultBad(_ResultMessage: "COMPANY_DOES_NOT_EXIST_ERROR", _ResultDetail: ""); 

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

                        foreach(var toSave in input.Users)
                        {
                            toSave.userAppId = "TBD"; // To create Key
                            toSave.password = ""; // Dont return the password                            
                            toSave.tokenAuth = "";

                            contextMVC.UserApp.Add(toSave);
                        }



                        //***** 4. Save and Commit to the Database (Atomic because is same Context DB) 
                        if (!error && autoCommit)
                        {
                            await contextMVC.SaveChangesAsync(); // Call it only once so do all other operations first
                            await contextApp.SaveChangesAsync(); // Call it only once so do all other operations first
                        }

                        //***** 5. Execute Send e-mails or other events once the database has been succesfully saved
                        //***** If this task fails, there are options -> 1. Retry multiple times 2. Save the event as Delay, 3.Rollback Database, Re

                        //***** 6. Confirm the Result (Pass | Fail) If gets to here there are not errors then return the new data from database
                        _output.ResultConfirmation = resultConfirmation.resultGood(_ResultMessage: "USER_SUCESSFULLY_CREATED"); // If OK
                        //_output.token = tokenString; // The token                        
                        //_output.user = input.user;
                    }// if (!error)
                }
                finally
                {
                    // If the context Father is null the context was created on his own, so dispose it
                    if (contextApp != null && contextFatherApp == null)
                    {
                        contextApp.Dispose();
                    }
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
                _output = new CreateUsersForCompanyTxn_Output(); // Restart variable to avoid returning any already saved data
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
