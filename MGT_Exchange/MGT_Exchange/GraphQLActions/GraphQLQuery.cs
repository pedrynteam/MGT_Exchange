using HotChocolate.Types;
using MGT_Exchange.AuthAPI.GraphQL;
using MGT_Exchange.AuthAPI.MVC;
using MGT_Exchange.ChatAPI.GraphQL;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Models;
using MGT_Exchange.ParticipantAPI.GraphQL;
using MGT_Exchange.TicketAPI.GraphQL;
using MGT_Exchange.TicketAPI.MVC;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.GraphQLActions
{
    // Uses GraphQL Types - Hot Chocolate combines MVCModel with GraphQLTypes (Sweet!)
    public class GraphQLQuery
    {
        private readonly MVCDbContext contextMVC;

        public GraphQLQuery(MVCDbContext _contextMVC)
        {
            contextMVC = _contextMVC;
        }

        // Get all users for a Company by CompanyID
        public async Task<Company> GetCompanyAsync(string id)
        {
            return await contextMVC.Company.Where(q => q.CompanyId.Equals(id)).FirstOrDefaultAsync();
        }

        // Get a ticket by TicketId
        public async Task<Ticket> GetTicketAsync(int id)
        {
            return await contextMVC.Ticket.FindAsync(id);
        }

        // Get a chat by ChatId
        public async Task<Chat> GetChatAsync(int id)
        {
            return await contextMVC.Chat.FindAsync(id);
        }

        // Get all chats for testing only
        public async Task<List<Chat>> GetChatsAsync()
        {
            return await contextMVC.Chat.ToListAsync();
        }

        // Get all users for testing only
        public async Task<List<UserApp>> GetUserAppsAsync()
        {
            return await contextMVC.UserApp.ToListAsync();
        }

        // Get all participants for testing only
        public async Task<List<Participant>> GetParticipantsAsync()
        {
            return await contextMVC.Participant.ToListAsync();
        }

        // Get all comments for testing only
        public async Task<List<Comment>> GetCommentsAsync()
        {
            return await contextMVC.Comment.ToListAsync();
        }

    }// public class GraphQLQuery

    public class GraphQLQueryType : ObjectType<GraphQLQuery>
    {
        protected override void Configure(IObjectTypeDescriptor<GraphQLQuery> descriptor)
        {
            descriptor.Field(t => t.GetTicketAsync(default))
                .Type<TicketType>()
                .Argument("id", a => a.Type<NonNullType<IntType>>())
                .Name("ticket")
                 //.Directive(new AuthorizeDirective()) // This is like Authenticated only
                 //.Authorize(policy: "OnlyManagersDb") // This is using a policy
            ;

            descriptor.Field(t => t.GetCompanyAsync(default))    
                .Type<CompanyType>()    
                .Argument("id", a => a.Type<NonNullType<StringType>>())    
                .Name("company")
                //.Directive(new AuthorizeDirective()) // This is like Authenticated only
            //.Authorize(policy: "OnlyManagersDb") // This is using a policy
            ;

            descriptor.Field(t => t.GetChatAsync(default))        
                .Type<ChatType>()    
                .Argument("id", a => a.Type<NonNullType<IntType>>())    
                .Name("chat")
                //.Directive(new AuthorizeDirective()) // This is like Authenticated only
            //.Authorize(policy: "OnlyManagersDb") // This is using a policy
            ;

            descriptor.Field(t => t.GetCommentsAsync())
                .Type<ListType<CommentType>>()
                .Name("comments")
            //.Directive(new AuthorizeDirective()) // This is like Authenticated only
            //.Authorize(policy: "OnlyManagersDb") // This is using a policy
            ;

            descriptor.Field(t => t.GetParticipantsAsync())
                .Type<ListType<ParticipantType>>()
                .Name("participants")
            //.Directive(new AuthorizeDirective()) // This is like Authenticated only
            //.Authorize(policy: "OnlyManagersDb") // This is using a policy
            ;

            descriptor.Field(t => t.GetChatsAsync())
    .Type<ListType<ChatType>>()
    .Name("chats")
            //.Directive(new AuthorizeDirective()) // This is like Authenticated only
//.Authorize(policy: "OnlyManagersDb") // This is using a policy
;

                        descriptor.Field(t => t.GetUserAppsAsync())
    .Type<ListType<UserAppType>>()
    .Name("users")
            //.Directive(new AuthorizeDirective()) // This is like Authenticated only
//.Authorize(policy: "OnlyManagersDb") // This is using a policy
;

        }

    }// public class GraphQLQueryType : ObjectType<GraphQLQuery>
}
