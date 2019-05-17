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

        public async Task<AddCommentToChatTxn_Output> AddCommentToChatTxn(AddCommentToChatTxn_Input input, [Service]IEventSender eventSender, [Service]IEventRegistry eventRegistry)
        {
            AddCommentToChatTxn addCommentToChatTxn = new AddCommentToChatTxn();
            return await addCommentToChatTxn.Execute(input: input, eventSender: eventSender, eventRegistry: eventRegistry);
        }

        public async Task<AddCommentInfoToCommentTxn_Output> AddCommentInfoToCommentTxn(AddCommentInfoToCommentTxn_Input input, [Service]IEventSender eventSender)
        {
            AddCommentInfoToCommentTxn AddCommentInfoToCommentTxn = new AddCommentInfoToCommentTxn();
            return await AddCommentInfoToCommentTxn.Execute(input: input, eventSender: eventSender);
        }

        public async Task<UserEntersChatTxn_Output> UserEntersChatTxn(UserEntersChatTxn_Input input, [Service]IEventSender eventSender)
        {
            UserEntersChatTxn UserEntersChatTxn = new UserEntersChatTxn();
            return await UserEntersChatTxn.Execute(input: input, eventSender: eventSender);
        }

        public async Task<RetrieveMasterInformationByUser_Output> RetrieveMasterInformationByUser(RetrieveMasterInformationByUser_Input input, [Service]IEventSender eventSender)
        {
            RetrieveMasterInformationByUserTxn RetrieveMasterInformationByUser = new RetrieveMasterInformationByUserTxn();
            return await RetrieveMasterInformationByUser.Execute(input: input, eventSender: eventSender);
        }

        public async Task<CreateMockChatsTxn_Output> CreateMockChatsTxn(CreateMockChatsTxn_Input input, [Service]IServiceProvider serviceProvider)
        {
            CreateMockChatsTxn createMockChatsTxn = new CreateMockChatsTxn();
            return await createMockChatsTxn.Execute(input: input, serviceProvider: serviceProvider);
        }

        public async Task<LoadChatAndUpdateUnseenForUserTxn_Output> LoadChatAndUpdateUnseenForUserTxn(LoadChatAndUpdateUnseenForUserTxn_Input input, [Service]IEventSender eventSender)
        {
            LoadChatAndUpdateUnseenForUserTxn loadChatAndUpdateUnseenForUserTxn = new LoadChatAndUpdateUnseenForUserTxn();
            return await loadChatAndUpdateUnseenForUserTxn.Execute(input: input, eventSender: eventSender);
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

            descriptor.Field(t => t.AddCommentInfoToCommentTxn(default, default)) // From here the Injection works
                            .Type<NonNullType<AddCommentInfoToCommentTxn_OutputType>>()
                            .Argument("input", a => a.Type<NonNullType<AddCommentInfoToCommentTxn_InputType>>())
                            .Description("Create CommentInfo to Comment")
                            ;

            descriptor.Field(t => t.AddCommentToChatTxn(default, default, default)) // From here the Injection works
                .Type<NonNullType<AddCommentToChatTxn_OutputType>>()
                .Argument("input", a => a.Type<NonNullType<AddCommentToChatTxn_InputType>>())
                .Description("Create Comment to Chat")
                ;

            descriptor.Field(t => t.UserEntersChatTxn(default, default)) // From here the Injection works
                .Type<NonNullType<UserEntersChatTxn_OutputType>>()
                .Argument("input", a => a.Type<NonNullType<UserEntersChatTxn_InputType>>())
                .Description("User Enters Chat")
                ;

            descriptor.Field(t => t.RetrieveMasterInformationByUser(default, default)) // From here the Injection works
                .Type<NonNullType<RetrieveMasterInformationByUser_OutputType>>()
                .Argument("input", a => a.Type<NonNullType<RetrieveMasterInformationByUser_InputType>>())
                .Description("Get Master Information By User")
                ;

            descriptor.Field(t => t.LoadChatAndUpdateUnseenForUserTxn(default, default)) // From here the Injection works
                .Type<NonNullType<LoadChatAndUpdateUnseenForUserTxn_OutputType>>()
                .Argument("input", a => a.Type<NonNullType<LoadChatAndUpdateUnseenForUserTxn_InputType>>())
                .Description("Get chat and update unseen comments for user")
                ;

            descriptor.Field(t => t.CreateMockChatsTxn(default, default)) // From here the Injection works
    .Type<NonNullType<CreateMockChatsTxn_OutputType>>()
    .Argument("input", a => a.Type<NonNullType<CreateMockChatsTxn_InputType>>())
    .Description("Create MockChats")
    ;

        }
    }

}
