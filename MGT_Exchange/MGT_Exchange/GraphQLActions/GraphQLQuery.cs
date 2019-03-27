using HotChocolate.Types;
using MGT_Exchange.Models;
using MGT_Exchange.TicketAPI.GraphQL;
using MGT_Exchange.TicketAPI.MVC;
using System;
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

        // Get a ticket by TicketId
        public async Task<Ticket> GetTicketAsync(int id)
        {
            return await contextMVC.Ticket.FindAsync(id);
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

        }
    }// public class GraphQLQueryType : ObjectType<GraphQLQuery>
}
