using HotChocolate.Types;
using MGT_Exchange.AuthAPI.GraphQL;
using MGT_Exchange.AuthAPI.MVC;
using MGT_Exchange.ChatAPI.MVC;
using MGT_Exchange.Data;
using MGT_Exchange.Models;
using MGT_Exchange.ParticipantAPI.GraphQL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.ChatAPI.GraphQL
{
    public class ChatType : ObjectType<Chat>
    {
        protected override void Configure(IObjectTypeDescriptor<Chat> descriptor)
        {

            descriptor.Field(t => t.Created)
                .Type<DateTimeType>();

            descriptor.Field(t => t.Comments)    
                .Type<ListType<CommentType>>()  
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
                    return context.Service<MVCDbContext>().Comment.Where(x => x.ChatId == context.Parent<Chat>().ChatId).Where(x => x.CommentId > _index).OrderByDescending(y => y.CommentId).Take(_take);
                }    
                )    
                ;

            
            descriptor.Field(t => t.Participants)    
                            .Type<ListType<ParticipantType>>()  
                            .Name("participants")    
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
                                return context.Service<MVCDbContext>().Participant.Where(x => x.ChatId == context.Parent<Chat>().ChatId).Where(x => x.ParticipantId > _index).OrderByDescending(y => y.ParticipantId).Take(_take);
                            }    
                            )    
                            ;

            /* fox foreing keys
            descriptor.Field(t => t.CreatedBy)
                .Type<IdentityUserType>()
                .Name("createdByUser")
                .Resolver(async context =>
                {
                    // UserApp in MVCDbContext, User Account Context in ApplicationDB this for query simplicity and Database security
                    // This database dont contain any users, users are in Users / Account Database
                    return await context.Service<MVCDbContext>().UserApp.Where(x => x.UserAppId == context.Parent<Chat>().UserAppId).FirstOrDefaultAsync();
                    //var userDB = await context.Service<ApplicationDbContext>().Users.Where(x => x.Id == context.Parent<Chat>().UserId).FirstOrDefaultAsync();
                    // UserApp userApp = new UserApp { Id = userDB.Id, Email = userDB.Email, UserName = userDB.UserName };
                    //return userDB; // userApp;
                }
                )
                ;
                */

/*
            descriptor.Field(t => t.ChatStatus)
    .Type<ChatStatusType>()
    //.Name("chatStatus")
    .Resolver(context =>
    {
        return context.Service<MVCDbContext>().ChatStatus.Where(x => x.ChatStatusId == context.Parent<Chat>().ChatStatusId).FirstOrDefaultAsync();
    }
    )
    ;

            descriptor.Field(t => t.ChatKind)
    .Type<ChatKindType>()
    //.Name("chatStatus")
    .Resolver(context =>
    {
        return context.Service<MVCDbContext>().ChatKind.Where(x => x.ChatKindId == context.Parent<Chat>().ChatKindId).FirstOrDefaultAsync();
    }
    )
    ;

*/
        }
    }

    // Leave it empty, HotChocolate will take care of it
    public class ChatInputType : InputObjectType<Chat>
    {
        protected override void Configure(IInputObjectTypeDescriptor<Chat> descriptor)
        {

            descriptor.Field(t => t.Created)
                .Type<DateTimeType>();
            
        }
    }
}
