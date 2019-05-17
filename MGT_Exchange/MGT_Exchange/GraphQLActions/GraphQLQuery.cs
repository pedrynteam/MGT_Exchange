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

        // Get Comment by id
        public async Task<Comment> GetCommentAsync(int id)
        {
            return await contextMVC.Comment.Where(q => q.CommentId == id).FirstOrDefaultAsync();
        }

        // Get CommentInfo by id
        public async Task<CommentInfo> GetCommentInfoAsync(int id)
        {
            return await contextMVC.CommentInfo.Where(q => q.CommentInfoId == id).FirstOrDefaultAsync();
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

        // Get Notifications by User
        public async Task<List<Notification>> GetNotificationsByUserAsync(string toUserAppId, int take = 10)
        {
            List<Notification> notifications = new List<Notification>();

            notifications = await contextMVC.Notification.Where(x => x.ToUserAppId.Equals(toUserAppId)).OrderBy(y => y.Seen).ThenByDescending(y => y.NotificationId).Take(take).ToListAsync();

            /*
            Notification notification = contextMVC.Notification.Where(x => x.ToUserAppId.Equals(toUserAppId)).FirstOrDefault();
            notification.Seen = true;

            contextMVC.Notification.Update(notification);
            await contextMVC.SaveChangesAsync();
            */

            return notifications;
        }

        // Get Chats by User
        public async Task<List<Chat>> GetChatsByUserAsync(string userAppId, int take = 10)
        {
            // List<Chat> chats = new List<Chat>();

            /*
            Chat chatDb = await contextMVC.Chat
                        .Where(x => x.ChatId == input.Comment.ChatId)
                        .Where(x => x.Participants.Any(y => y.UserAppId.Equals(input.Comment.UserAppId)))
                        .Include(o => o.Participants)
                        //.ThenInclude(i => i.User)                        
                        .FirstOrDefaultAsync();
                        */

            List<Chat> chats = await contextMVC.Chat
                        .Where(x => x.Participants.Any(y => y.UserAppId.Equals(userAppId)))
                        .OrderByDescending(x => x.UpdatedAt)
                        .Take(take)
                        .ToListAsync();

            return chats;

            /*
            var query = from user in contextMVC.Chat
                        select user;
                        


            notifications = await contextMVC.Notification.Where(x => x.ToUserAppId.Equals(toUserAppId)).OrderBy(y => y.Seen).ThenByDescending(y => y.NotificationId).Take(take).ToListAsync();

            /*
            Notification notification = contextMVC.Notification.Where(x => x.ToUserAppId.Equals(toUserAppId)).FirstOrDefault();
            notification.Seen = true;

            contextMVC.Notification.Update(notification);
            await contextMVC.SaveChangesAsync();
            */

            // return notifications;
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

            descriptor.Field(t => t.GetCommentAsync(default))
                .Type<CommentType>()
                .Argument("id", a => a.Type<NonNullType<IntType>>())
                .Name("comment")
            //.Directive(new AuthorizeDirective()) // This is like Authenticated only
            //.Authorize(policy: "OnlyManagersDb") // This is using a policy
            ;

            descriptor.Field(t => t.GetCommentsAsync())
                .Type<ListType<CommentType>>()
                .Name("comments")
            //.Directive(new AuthorizeDirective()) // This is like Authenticated only
            //.Authorize(policy: "OnlyManagersDb") // This is using a policy
            ;

            descriptor.Field(t => t.GetCommentInfoAsync(default))
                .Type<ListType<CommentInfoType>>()
                .Argument("id", a => a.Type<NonNullType<IntType>>())
                .Name("commentsInfo");

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


            descriptor.Field(t => t.GetNotificationsByUserAsync(default, default))
                .Type<ListType<NotificationType>>()
                .Argument("toUserAppId", a => a.Type<NonNullType<StringType>>())
                .Argument("take", a => a.Type<IntType>())
                .Name("notificationsByUser")
                ;

            descriptor.Field(t => t.GetChatsByUserAsync(default, default))
                .Type<ListType<ChatType>>()
                .Argument("userAppId", a => a.Type<NonNullType<StringType>>())
                .Argument("take", a => a.Type<IntType>())
                .Name("chatsByUser")
                ;



        }

    }// public class GraphQLQueryType : ObjectType<GraphQLQuery>
}
