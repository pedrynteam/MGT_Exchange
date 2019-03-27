using HotChocolate.Types;
using MGT_Exchange.Models;
using MGT_Exchange.TicketAPI.MVC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.TicketAPI.GraphQL
{
    public class TicketType : ObjectType<Ticket>
    {
        protected override void Configure(IObjectTypeDescriptor<Ticket> descriptor)
        {
            descriptor.Field(x => x.TicketId);
            descriptor.Field(x => x.Name);
            descriptor.Field(t => t.Comments)
                .Type<ListType<CommentTicketType>>()
                .Name("comments")
                .Argument("index", a => a.Type<IntType>())
                .Argument("take", a => a.Type<IntType>())    
                .Resolver(context =>    
                {
                    int _index = context.Argument<int>("index");

                    int _take = context.Argument<int>("take");
                    _take = _take == 0 ? 10 : _take;

                    // 1. Where index based
                    // 2. Order By
                    // 3. Take 
                    return context.Service<MVCDbContext>().CommentTicket.Where(x => x.TicketId == context.Parent<Ticket>().TicketId).Where(x => x.commentTicketId > _index).OrderByDescending(y => y.commentTicketId).Take(_take);
                }                    
                );


        }
    }
}
