using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MGT_Exchange.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MGT_Exchange.Models;
using HotChocolate;
using HotChocolate.AspNetCore;
using HotChocolate.Subscriptions;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MGT_Exchange.AuthAPI.Transactions;
using System.Security.Claims;

namespace MGT_Exchange
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddDefaultUI(UIFramework.Bootstrap4)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddDbContext<MVCDbContext>(options =>
                    options.UseSqlServer(Configuration.GetConnectionString("MVCDbContext")));

            // Add in-memory event provider - This is for subscriptions. NuGet Package HotChocolate.Subscriptions.InMemory
            var eventRegistry = new InMemoryEventRegistry();
            services.AddSingleton<IEventRegistry>(eventRegistry);
            services.AddSingleton<IEventSender>(eventRegistry);

            // https://jonhilton.net/security/apis/secure-your-asp.net-core-2.0-api-part-2---jwt-bearer-authentication/

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "https://localhost:44325",
                ValidAudience = "https://localhost:44325",
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes("superSecretKey@345"))//_configuration["SecurityKey"]))
            };
        });

            // this enables you to use DataLoader in your resolvers.
            services.AddDataLoaderRegistry();

            // Add GraphQL Services
            services.AddGraphQL(sp => Schema.Create(c =>
            {
                c.RegisterServiceProvider(sp);

                // Adds the authorize directive and
                // enables the authorization middleware.
                c.RegisterAuthorizeDirectiveType();
                c.RegisterQueryType<GraphQLActions.GraphQLQueryType>();
                c.RegisterMutationType<GraphQLActions.GraphQLMutationType>();
                c.RegisterSubscriptionType<GraphQLActions.GraphQLSubscriptionType>();
                c.RegisterExtendedScalarTypes(); //Needed to fix: CommentInput.created: Cannot resolve input-type `System.DateTime` - Type: CommentInput'
            }));

            services.AddAuthorization(options =>
            {
                // Using Token
                options.AddPolicy("CompletedTrainingToken", policy =>
                    policy.RequireAssertion(context =>
                        context.User.HasClaim("CompletedBasicTraining", "Yes")
                        ));

                // Using ApplicationDbContext Database claims
                options.AddPolicy("OnlyManagersDb", policy =>
                policy.Requirements.Add(
                    new ClaimInDatabaseRequirement(new Claim("EmployeeStatus", "Manager")))
                    );

            });

            services.AddSingleton<IAuthorizationHandler, ClaimInDatabaseHandler>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            /* this part is failing the playground and get Schema. Syn, Sync, Sync
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
            // */



            // enable this if you want tu support subscription.
            app.UseWebSockets();
            /*/
            // Had to not use /graphql because SAHB cannot support it
            app.UseGraphQL("/graphql");
            app.UseGraphQL();
            // enable this if you want to use graphiql instead of playground.
            // app.UseGraphiQL();
            app.UsePlayground("/graphql", "/playground"); //*/
           
            //*
            // enable this if you want to use graphiql instead of playground.
            // Use is in this way
            app.UseGraphQL();
            app.UseGraphiQL();
            app.UsePlayground();
            //*/



        }
    }
}
