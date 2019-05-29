using MGT_Exchange.Data;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MGT_Exchange.AuthAPI.Transactions
{

    public class ClaimInDatabaseRequirement : IAuthorizationRequirement
    {
        public Claim claim { get; set; }
        public ClaimInDatabaseRequirement(Claim claimIn)
        {
            claim = claimIn;
        }
    }

    public class ClaimInDatabaseHandler : AuthorizationHandler<ClaimInDatabaseRequirement>
    {
        protected override Task HandleRequirementAsync(
               AuthorizationHandlerContext context,
               ClaimInDatabaseRequirement requirement)
        {

            try
            {
                using (ApplicationDbContext _contextUsers = new ApplicationDbContext())
                {

                    Boolean hasClaim = _contextUsers.UserClaims.Any(x => x.UserId.Equals(context.User.Identity.Name)
                    && x.ClaimType.Equals(requirement.claim.Type)
                    && x.ClaimValue.Equals(requirement.claim.Value));

                    if (hasClaim)
                    {
                        context.Succeed(requirement);
                    }

                }
            }
            catch (Exception ex) // Main try 
            {
                System.Diagnostics.Debug.WriteLine("<ClaimInDatabaseHandler> Error: " + ex.Message);
                string innerError = (ex.InnerException != null) ? ex.InnerException.Message : "";
                System.Diagnostics.Debug.WriteLine("<ClaimInDatabaseHandler> Error Inner: " + innerError);
            }
            finally
            {
                // Save Logs if needed
            }

            return Task.CompletedTask;
        }
    }
}
