using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MGT_Exchange.AuthAPI.Resources
{
    static class LoginTokenFunction
    {
        private static string ConvertToBase32Date(DateTime dateIn)
        {
            string base32Out = "";

            Dictionary<int, char> base32dict = new Dictionary<int, char>
            {
                { 0, 'A' },
                { 1, 'B' },
                { 2, 'C' },
                { 3, 'D' },
                { 4, 'E' },
                { 5, 'F' },
                { 6, 'G' },
                { 7, 'H' },
                { 8, 'I' },
                { 9, 'J' },
                { 10, 'K' },
                { 11, 'L' },
                { 12, 'M' },
                { 13, 'N' },
                { 14, 'O' },
                { 15, 'P' },
                { 16, 'Q' },
                { 17, 'R' },
                { 18, 'S' },
                { 19, 'T' },
                { 20, 'U' },
                { 21, 'V' },
                { 22, 'W' },
                { 23, 'X' },
                { 24, 'Y' },
                { 25, 'Z' },
                { 26, '2' },
                { 27, '3' },
                { 28, '4' },
                { 29, '5' },
                { 30, '6' },
                { 31, '7' }
            };

            // Think about adding Hours and Minutes to the Date. And remove the first 2 of the year. Pending. for sure seconds will help
            /* Use Base32: 0=A ... 25=Z ... 26=2 ... 31=7
             * First Chunk Part: Date = 1998/01/30 = TJIB6
             *      Year:   1998 is divided in 3
             *              19=T base32, Maximum year 31 so 3100 year
             *               9=J base32, Minimum 0 (A) Maximum 9 (J)
             *               8=I base32, Minimum 0 (A) Maximum 9 (J)
             *      Month:  1-12 Divided in 1. 1 = B
             *               1=B base32, Minimum 1 (B) Maximum 12 (M)
             *      Day:    30 is divided in 1
             *              30=6 base32, Minimum 1 (B) Maximum 31 (7)  
             */

            String year = dateIn.Year.ToString();
            // Year:   1998 is divided in 3: 19 9 8            
            base32Out += base32dict[int.Parse(year.Substring(0, 2))]; //19
            base32Out += base32dict[int.Parse(year.Substring(2, 1))];   // 9
            base32Out += base32dict[int.Parse(year.Substring(3, 1))];   // 8
            // System.Diagnostics.Debug.WriteLine("Year: " + base32Out);

            // Month:  1-12 Divided in 1. 1 = B
            String month = dateIn.Month.ToString();
            base32Out += base32dict[int.Parse(month)];   // 1
            // System.Diagnostics.Debug.WriteLine("Year & Month: " + base32Out);

            // Day:    1-31 is divided in 1. 30 = 6
            String day = dateIn.Day.ToString();
            base32Out += base32dict[int.Parse(day)];   // 1
            // System.Diagnostics.Debug.WriteLine("Year & Month & Day: " + base32Out);

            return base32Out;
        }

        private static string ConvertToBase32Sequence(string base32In)
        {
            string base32Out = "";

            // Uses only numbers
            Dictionary<int, char> base32dict = new Dictionary<int, char>
            {
                { '0', 'A' },
                { '1', 'B' },
                { '2', 'C' },
                { '3', 'D' },
                { '4', 'E' },
                { '5', 'F' },
                { '6', 'G' },
                { '7', 'H' },
                { '8', 'I' },
                { '9', 'J' }
            };

            foreach (char number in base32In)
            {
                char value;

                if (base32dict.TryGetValue(number, out value))
                {
                    base32Out += value;
                }
            }

            return base32Out;
        }

        public static string CreateTokenId(long sequenceIn, DateTime dateIn)
        {

            /* 
             * Convert the number to the following format AAAAA-AAAAA-AAAAA
             * Use Base32: 0=A ... 25=Z ... 26=2 ... 31=7
             * First Chunk Part: Date = 1998/01/30 = TJIB6
             *      Year:   1998 is divided in 3
             *              19=T base32, Maximum year 31 so 3100 year
             *               9=J base32, Minimum 0 (A) Maximum 9 (J)
             *               8=I base32, Minimum 0 (A) Maximum 9 (J)
             *      Month:  1-12 Divided in 1. 1 = B
             *               1=B base32, Minimum 1 (B) Maximum 12 (M)
             *      Day:    30 is divided in 1
             *              30=6 base32, Minimum 1 (B) Maximum 31 (7)             
             * Second and Third chunk are the composite for the sequence
             *      Logic:  1. Left Pad with zeros for 10 characters
             *              2. Convert to Base32 each number (zeroes included)
             *              3. Inverse the result string (it's easier to see and then AAAA, then count how many A's at the beginning.
             *              4. Chunk by 5
             *              5. Minimum: 0000000001 Maximum: 9,999,999,999
             *              6. Minimum: AAAAAAAAAB Maximum: JJJJJJJJJJ
             *              7. Inverse: BAAAAAAAAA Maximum: JJJJJJJJJJ
             *              8. Eliminate latest Chunk if is empty = -AAAA
             *      Samples:
             *              21 = 0000000021 = AAAAAAAACB = BCAAAAAAAAAA = BCAAA-AAAAA
             *              123456 = 0000123456 = AAAABCDEFG = GFEDCBAAAA = GFEDC-BAAAA
             *              76554321 = 0007654321 = AAAHGFEDCB = BCDEFGHAAA = BCDEF-GHAAA
             *              9876543210 = 987654321 = JIHGFEDCBA = ABCDEFGHIJ = ABCDE-FGHIJ
             * Result for 21 created on 1998/01/30: TJIB6-BCAAA-AAAAA        
             * */

            // 1.Left Pad with zeros for 10 characters
            string tokenId = sequenceIn.ToString().PadLeft(10, '0');
            System.Diagnostics.Debug.WriteLine("1. " + tokenId);
            // 2. Convert to Base32 each number (zeroes included)
            tokenId = ConvertToBase32Sequence(tokenId);
            System.Diagnostics.Debug.WriteLine("2. " + tokenId);
            // 3. Inverse the result string (it's easier to see and then AAAA, then count how many A's at the beginning.
            var inverseStr = tokenId.ToCharArray();
            Array.Reverse(inverseStr);
            tokenId = new string(inverseStr);
            System.Diagnostics.Debug.WriteLine("3. " + tokenId);
            // 4. Chunk by 5
            tokenId = tokenId.Insert(5, "-");
            System.Diagnostics.Debug.WriteLine("4. " + tokenId);

            // 5. Create Date Chunk
            DateTime now = DateTime.Now;
            // now = new DateTime(1998, 1, 31);            
            string tokenDate = ConvertToBase32Date(dateIn);
            System.Diagnostics.Debug.WriteLine("5. " + tokenDate);

            // 6. Create Final Token with dates
            tokenId = tokenDate + "-" + tokenId;
            System.Diagnostics.Debug.WriteLine("6. " + tokenId);

            return tokenId;
        }
    }
/*
        public static TxnStatus DefaultTxnStatus(int _TokenTxnId, string _TokenTxnName, string _Name, bool _IsOnline, DateTime _Inserted, DateTime _Updated, int _TxnStatusId = 0, string _Status = "NEW", string _Message = "", string _Detail = "", bool _IsPassed = false, bool _IsToRetry = false,
            string _Owner = "", string _Reference = "", int _Tries = 1)
        {

            // DateTime _Inserted, DateTime _Updated, 
            return new TxnStatus
            {
                // Mandatory: _TokenTxnId, _TokenTxnName, _Name, _IsOnline, _Inserted, _Updated

                TxnStatusId = _TxnStatusId,
                TokenTxnName = _TokenTxnName,

                Name = _Name, // Purchase
                Status = _Status, // PASS / FAIL / RETRY / NEW / PROCESSING
                Message = _Message, // ORDER_NOT_FOUND | The order was not found in the system
                Detail = _Detail, // (Order Number)

                // For easy querying
                IsPassed = _IsPassed,// True | False
                IsOnline = _IsOnline,// True | False
                IsToRetry = _IsToRetry,// True | False

                // For information
                Owner = _Owner,// Who needs to execute the event when is delayed
                Reference = _Reference,// 
                Inserted = _Inserted,// 
                Updated = _Updated,//         
                Tries = _Tries,//  

                TokenTxnId = _TokenTxnId

            };

        }// public TxnStatus DefaultTxnStatus

        public static TxnStatusDetail DefaultTxnStatusDetail(int _TxnStatusId, int _TokenTxnId, string _TokenTxnName, int _TxnSequence, string _Name, bool _IsOnline, DateTime _Inserted, DateTime _Updated, int _TxnStatusDetailId = 0, string _Status = "NEW", string _Message = "", string _Detail = "", bool _IsPassed = false, bool _IsToRetry = false,
            string _Owner = "", string _Reference = "", int _Tries = 1)
        {

            // DateTime _Inserted, DateTime _Updated, 
            return new TxnStatusDetail
            {
                // Mandatory: _TxnStatusId, _TokenTxnName, _TokenTxnId, _Name, _IsOnline, _Inserted, _Updated

                TxnStatusDetailId = _TxnStatusDetailId,

                TxnSequence = _TxnSequence,
                Name = _Name, // Purchase
                Status = _Status, // PASS / FAIL / RETRY / NEW / PROCESSING
                Message = _Message, // ORDER_NOT_FOUND | The order was not found in the system
                Detail = _Detail, // (Order Number)

                // For easy querying
                IsPassed = _IsPassed,// True | False
                IsOnline = _IsOnline,// True | False
                IsToRetry = _IsToRetry,// True | False

                // For information
                Owner = _Owner,// Who needs to execute the event when is delayed
                Reference = _Reference,// 
                Inserted = _Inserted,// 
                Updated = _Updated,//         
                Tries = _Tries,//  

                TokenTxnId = _TokenTxnId,
                TokenTxnName = _TokenTxnName,

                TxnStatusId = _TxnStatusId,

            };

        }// public TxnStatus DefaultTxnStatusDetail

        public static TxnStatusCall DefaultTxnStatusCall(int _TokenTxnId, string _TokenTxnName, DateTime _Inserted, int _TxnStatusCallId = 0, string _RequestMessage = "",
            string _ResponseMessage = "", string _Host = "", string _IP = "", string _Reference = "")
        {

            // DateTime _Inserted, DateTime _Updated, 
            return new TxnStatusCall
            {

                // Mandatory: _TokenTxnName, _TokenTxnId, _Inserted

                TxnStatusCallId = _TxnStatusCallId,
                RequestMessage = _RequestMessage,
                ResponseMessage = _ResponseMessage,
                Host = _Host,
                IP = _IP,
                Reference = _Reference,
                Inserted = _Inserted,

                TokenTxnId = _TokenTxnId,
                TokenTxnName = _TokenTxnName

            };

        }// public TxnStatus DefaultTxnStatusDetail

        public static async Task<TxnStatus> CreateOrUpdateTxnStatusAndToken(string _Name = "", TxnStatus _txnStatus = null, ResultConfirmation _resultConfirmation = null, TxnStatusCall _TxnStatusCall = null)
        {
            DateTime dateNow = DateTime.UtcNow;
            TokenTxn _token;

            using (var _contextTxn = new MVCDbContext()) // Create Token and Transaction Events for log
            {
                if (_txnStatus == null) // Create
                {
                    _token = new TokenTxn { TokenTxnId = 0, Name = "", Type = "TxnHandler", Used = false, Inserted = dateNow };

                    _contextTxn.Add(_token);
                    await _contextTxn.SaveChangesAsync();

                    _token.Name = CreateTokenId(_token.TokenTxnId, dateNow);
                    _token.Used = true;
                    _contextTxn.Update(_token);

                    await _contextTxn.SaveChangesAsync();

                    // Create TxnStatus
                    _txnStatus = DefaultTxnStatus(_TokenTxnId: _token.TokenTxnId, _TokenTxnName: _token.Name, _Name: _Name, _IsOnline: true, _Inserted: dateNow, _Updated: dateNow);


                    // _txnStatus.TxnStatusCall = DefaultTxnStatusCall(_TokenTxnId: _token.TokenTxnId, _TokenTxnName: _token.Name, _Inserted: dateNow);

                    // Add the Txn Status Call
                    if (_TxnStatusCall != null)
                    {
                        _TxnStatusCall.TokenTxnId = _token.TokenTxnId;
                        _TxnStatusCall.TokenTxnName = _token.Name;
                        _txnStatus.TxnStatusCall = _TxnStatusCall;
                    }

                    _contextTxn.Add(_txnStatus);
                }
                else // we are getting an txnStatus
                {
                    if (_resultConfirmation != null)
                    {
                        _txnStatus.IsPassed = _resultConfirmation.ResultPassed;
                        _txnStatus.Status = _resultConfirmation.ResultCode;
                        _txnStatus.Message = _resultConfirmation.ResultMessage;
                        _txnStatus.Detail = _resultConfirmation.ResultDetail;
                    }

                    if (_txnStatus.TxnStatusId == 0) // txnStatus NEW created at source
                    {
                        _contextTxn.Add(_txnStatus);
                    }
                    else
                    {
                        _txnStatus.Updated = dateNow;
                        _contextTxn.Update(_txnStatus);
                    }
                }

                /*
                txnStatus.TxnStatusDetails = new List<TxnStatusDetail>();
                txnStatus.TxnStatusDetails.Add(DefaultTxnStatusDetail(_TxnSequence: 1, _Name: "Inventory", _TxnStatusId: txnStatus.TxnStatusId, _TokenTxnId: _token.TokenTxnId, _TokenTxnName: _token.Name, _IsOnline: true, _Inserted: dateNow, _Updated: dateNow));
                txnStatus.TxnStatusDetails.Add(DefaultTxnStatusDetail(_TxnSequence: 2, _Name: "DecreaseLot", _TxnStatusId: txnStatus.TxnStatusId, _TokenTxnId: _token.TokenTxnId, _TokenTxnName: _token.Name, _IsOnline: true, _Inserted: dateNow, _Updated: dateNow));
                txnStatus.TxnStatusDetails.Add(DefaultTxnStatusDetail(_TxnSequence: 3, _Name: "SendEmail", _TxnStatusId: txnStatus.TxnStatusId, _TokenTxnId: _token.TokenTxnId, _TokenTxnName: _token.Name, _IsOnline: true, _Inserted: dateNow, _Updated: dateNow));
                */
/*
                await _contextTxn.SaveChangesAsync();
            }// using (var _contextMVCM = new MVCDbContext()) // Create Token and Transaction Events for log

            return _txnStatus;

        }// public async Task<TxnStatus> CreateTxnStatusAndToken()

        public static async Task<TxnStatusDetail> CreateOrUpdateTxnStatusDetail(TxnStatusDetail _txnStatusDetail, ResultConfirmation _resultConfirmation = null)
        {
            using (var _contextTxn = new MVCDbContext())
            {
                if (_resultConfirmation != null)
                {
                    _txnStatusDetail.IsPassed = _resultConfirmation.ResultPassed;
                    _txnStatusDetail.Status = _resultConfirmation.ResultCode;
                    _txnStatusDetail.Message = _resultConfirmation.ResultMessage;
                    _txnStatusDetail.Detail = _resultConfirmation.ResultDetail;
                }

                if (_txnStatusDetail.TxnStatusDetailId == 0)
                {
                    _contextTxn.Add(_txnStatusDetail);
                }
                else
                {
                    _txnStatusDetail.Updated = DateTime.Now;
                    _contextTxn.Update(_txnStatusDetail);
                }

                await _contextTxn.SaveChangesAsync();

            }// using (var _contextMVCM = new MVCDbContext())

            return _txnStatusDetail;
        }// public async Task<_txnStatusDetail> CreateOrUpdateTxnStatusDetail()

        public static async Task<TxnStatusCall> CreateOrUpdateTxnCall(TxnStatusCall _txnStatusCall)
        {
            using (var _contextTxn = new MVCDbContext())
            {
                if (_txnStatusCall.TxnStatusCallId == 0)
                {
                    _contextTxn.Add(_txnStatusCall);
                }
                else
                {
                    _txnStatusCall.Inserted = DateTime.Now;
                    _contextTxn.Update(_txnStatusCall);
                }

                await _contextTxn.SaveChangesAsync();

            }// using (var _contextMVCM = new MVCDbContext())

            return _txnStatusCall;
        }// public async Task<TxnStatusCall> CreateOrUpdateTxnCall()

    }
    */
}
