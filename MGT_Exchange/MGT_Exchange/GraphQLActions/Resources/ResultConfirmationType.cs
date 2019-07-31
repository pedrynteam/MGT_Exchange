using HotChocolate.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.GraphQLActions.Resources
{
    // Class to handle confirmations to client, PASS / FAIL, Message
    public class resultConfirmation
    {
        public string resultCode { get; set; }
        public bool resultPassed { get; set; }
        public string resultMessage { get; set; } // Message to translate: ORDER_NOT_FOUND
        public string resultDetail { get; set; } // Detail to transaction: 1   (Order number)
        public List<itemKey> resultDictionary { get; set; }

        public static resultConfirmation resultGood(string _ResultMessage, string _ResultDetail = "", List<itemKey> _ResultDictionary = null)
        {
            return new resultConfirmation
            {
                resultCode = "PASS",
                resultPassed = true,
                resultMessage = _ResultMessage,
                resultDetail = _ResultDetail,
                resultDictionary = _ResultDictionary
            };
        }

        public static resultConfirmation resultBad(string _ResultMessage, string _ResultDetail = "", List<itemKey> _ResultDictionary = null)
        {
            return new resultConfirmation
            {
                resultCode = "FAIL",
                resultPassed = false,
                resultMessage = _ResultMessage,
                resultDetail = _ResultDetail,
                resultDictionary = _ResultDictionary
            };
        }
    }

    public class ResultConfirmationType : ObjectType<resultConfirmation>
    {
        protected override void Configure(IObjectTypeDescriptor<resultConfirmation> descriptor)
        {

            descriptor.Field(t => t.resultCode)
                .Description("The response code of the result. PASS / FAIL");

            descriptor.Field(t => t.resultPassed)
                .Description("The status of the result: true / false");

            descriptor.Field(t => t.resultMessage)
                .Description("The message of the result");

            descriptor.Field(t => t.resultDetail)
                .Description("The detail of the result");

            descriptor.Field(t => t.resultDictionary)
                .Type<ListType<ItemKeyType>>()
                .Description("The dictionary of the result. Tag -> Value")
                .Resolver(context =>
                {
                    return context.Parent<resultConfirmation>().resultDictionary;
                }
                )
                ;

        }
    }

}
