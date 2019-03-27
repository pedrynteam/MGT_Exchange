using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.GraphQLActions.Resources
{
    // Class to handle confirmations to client, PASS / FAIL, Message
    public class ResultConfirmation
    {
        public string ResultCode { get; set; }
        public bool ResultPassed { get; set; }
        public string ResultMessage { get; set; } // Message to translate: ORDER_NOT_FOUND
        public string ResultDetail { get; set; } // Detail to transaction: 1   (Order number)
        public List<ItemKey> ResultDictionary { get; set; }

        public static ResultConfirmation resultGood(string _ResultMessage, string _ResultDetail = "", List<ItemKey> _ResultDictionary = null)
        {
            return new ResultConfirmation
            {
                ResultCode = "PASS",
                ResultPassed = true,
                ResultMessage = _ResultMessage,
                ResultDetail = _ResultDetail,
                ResultDictionary = _ResultDictionary
            };
        }

        public static ResultConfirmation resultBad(string _ResultMessage, string _ResultDetail = "", List<ItemKey> _ResultDictionary = null)
        {
            return new ResultConfirmation
            {
                ResultCode = "FAIL",
                ResultPassed = false,
                ResultMessage = _ResultMessage,
                ResultDetail = _ResultDetail,
                ResultDictionary = _ResultDictionary
            };
        }
    }

    public class ResultConfirmationType : ObjectType<ResultConfirmation>
    {
        protected override void Configure(IObjectTypeDescriptor<ResultConfirmation> descriptor)
        {

            descriptor.Field(t => t.ResultCode)
                .Description("The response code of the result. PASS / FAIL");

            descriptor.Field(t => t.ResultPassed)
                .Description("The status of the result: true / false");

            descriptor.Field(t => t.ResultMessage)
                .Description("The message of the result");

            descriptor.Field(t => t.ResultDetail)
                .Description("The detail of the result");

            descriptor.Field(t => t.ResultDictionary)
                .Type<ListType<ItemKeyType>>()
                .Description("The dictionary of the result. Tag -> Value")
                .Resolver(context =>
                {
                    return context.Parent<ResultConfirmation>().ResultDictionary;
                }
                )
                ;

        }
    }

}
