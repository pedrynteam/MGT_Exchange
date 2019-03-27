using HotChocolate.Types;
using MGT_Exchange.Models;
using MGT_Exchange.TicketAPI.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.TicketAPI.GraphQL
{
    public class CommentTicketType : ObjectType<CommentTicket>
    {
        protected override void Configure(IObjectTypeDescriptor<CommentTicket> descriptor)
        {
            descriptor.Field(x => x.commentTicketId);
            descriptor.Field(x => x.Message);
            descriptor.Field(t => t.Ticket)
                .Type<TicketType>()
                .Name("ticket")
                .Resolver(context => context.Service<MVCDbContext>().Ticket.FindAsync(context.Parent<CommentTicket>().TicketId))
                ;
        }
    }


}
