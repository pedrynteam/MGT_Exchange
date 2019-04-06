using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HotChocolate;
using HotChocolate.Subscriptions;
using HotChocolate.Types;
using MGT_Exchange.AuthAPI.Transactions;
using MGT_Exchange.ChatAPI.Transactions;

namespace MGT_Exchange.GraphQLActions
{
    public class GraphQLMutation
    {

        public async Task<CreateIdentityUserTxn_Output> CreateIdentityUserTxn(CreateIdentityUserTxn_Input input, [Service]IServiceProvider serviceProvider)
        {
            CreateIdentityUserTxn createIdentityUserTxn = new CreateIdentityUserTxn();
            return await createIdentityUserTxn.Execute(input: input, serviceProvider: serviceProvider);
        }

        public async Task<CreateTokenTxn_Output> CreateTokenTxn(CreateTokenTxn_Input input, [Service]IServiceProvider serviceProvider)
        {
            CreateTokenTxn_Output output = await new CreateTokenTxn().Execute(input: input, serviceProvider: serviceProvider);
            return output;
        }

        public async Task<CreateCompanyTxn_Output> CreateCompanyTxn(CreateCompanyTxn_Input input, [Service]IServiceProvider serviceProvider)
        {
            CreateCompanyTxn createCompanyTxn = new CreateCompanyTxn();
            return await createCompanyTxn.Execute(input: input, serviceProvider: serviceProvider);
        }

        public async Task<CreateUserTxn_Output> CreateUserTxn(CreateUserTxn_Input input, [Service]IServiceProvider serviceProvider)
        {
            CreateUserTxn createUserTxn = new CreateUserTxn();
            return await createUserTxn.Execute(input: input, serviceProvider: serviceProvider);
        }

        public async Task<CreateChatTxn_Output> CreateChatTxn(CreateChatTxn_Input input)
        {
            CreateChatTxn createChatTxn = new CreateChatTxn();
            return await createChatTxn.Execute(input: input);
        }

        public async Task<AddCommentToChatTxn_Output> AddCommentToChatTxn(AddCommentToChatTxn_Input input)
        {
            AddCommentToChatTxn addCommentToChatTxn = new AddCommentToChatTxn();
            return await addCommentToChatTxn.Execute(input: input);
        }

    }

    public class GraphQLMutationType : ObjectType<GraphQLMutation>
    {
        protected override void Configure(IObjectTypeDescriptor<GraphQLMutation> descriptor)
        {
            /*
            descriptor.Field(t => t.CreateCustomerAndOrdersTxn(default, default))
                .Type<NonNullType<CreateCustomerAndOrders_OutputType>>()
                .Argument("input", a => a.Type<NonNullType<CreateCustomerAndOrders_InputType>>())
                .Description("Create a new customer with orders")
                ;

            descriptor.Field(t => t.AddOrdersToCustomerTxn(default, default))
                .Type<NonNullType<AddOrdersToCustomerTxn_OutputType>>()
                .Argument("input", a => a.Type<NonNullType<AddOrdersToCustomerTxn_InputType>>())
                .Description("Add orders to a customer")
                ;
                */

            //*****   AuthAPI Transactions
            descriptor.Field(t => t.CreateIdentityUserTxn(default, default)) // From here the Injection works
                .Type<NonNullType<CreateIdentityUserTxn_OutputType>>()
                .Argument("input", a => a.Type<NonNullType<CreateIdentityUserTxn_InputType>>())
                .Description("Create Identity User")
                ;

            descriptor.Field(t => t.CreateCompanyTxn(default, default)) // From here the Injection works
                .Type<NonNullType<CreateCompanyTxn_OutputType>>()
                .Argument("input", a => a.Type<NonNullType<CreateCompanyTxn_InputType>>())
                .Description("Create Company")
                ;

            descriptor.Field(t => t.CreateUserTxn(default, default)) // From here the Injection works
                .Type<NonNullType<CreateUserTxn_OutputType>>()
                .Argument("input", a => a.Type<NonNullType<CreateUserTxn_InputType>>())
                .Description("Create User")
                ;

            descriptor.Field(t => t.CreateTokenTxn(default, default))
                .Type<NonNullType<CreateTokenTxn_OutputType>>()
                .Argument("input", a => a.Type<NonNullType<CreateTokenTxn_InputType>>())
                .Description("Create Token for user")
                ;
            //*****   ChatAPI Transactions
            descriptor.Field(t => t.CreateChatTxn(default))    
                .Type<NonNullType<CreateChatTxn_OutputType>>()    
                .Argument("input", a => a.Type<NonNullType<CreateChatTxn_InputType>>())    
                .Description("Create Chat Room")    
                ;

            descriptor.Field(t => t.AddCommentToChatTxn(default))
                .Type<NonNullType<AddCommentToChatTxn_OutputType>>()
                .Argument("input", a => a.Type<NonNullType<AddCommentToChatTxn_InputType>>())
                .Description("Create Comment to Chat")
                ;
        }
    }

}
