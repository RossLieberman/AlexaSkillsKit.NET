using System;

namespace AlexaSkillsKit.Tests
{
    public class AlexaSkillsKitTestBase
    {
        #region Message Properties
        internal readonly string SessionIdString = "SessionId.{0}";
        protected readonly string ApplicationId = "amzn1.echo-sdk-ams.app.aae66588-a121-44b8-bb59-6b3e5b539632";
        protected readonly string UserId = "amzn1.ask.account.FAKERANDOMGENERATED5WVYFZDTAKKC9UIR3NXUHYL1W9VMYWDNTH3ZPWBHZUVSIC5YS5D87BZRDYQGNAY1XZIAEJPLC5WEX2GCSECQ56FXX6HPT3LQBIZXRB5MC4VBPMUTADDHTHFDHRMXF7ILK4IRXKE4ABGIWUJNBOMIH7ETYRRBO6DR68IBWUT4YIXMS9SWHZIBD4YXVSVA";
        internal readonly string RequestIdString = "EdwRequestId.{0}";

        private string _sessionId;
        protected string SessionId
        {
            get
            {
                if (string.IsNullOrEmpty(_sessionId))
                    _sessionId = new Guid().ToString().ToLower();

                return string.Format(SessionIdString, _sessionId);
            }
            set { _sessionId = value; }
        }

        private string _requestId;
        protected string RequestId
        {
            get
            {
                if (string.IsNullOrEmpty(_requestId))
                    _requestId = new Guid().ToString().ToLower();

                return string.Format(RequestIdString, _requestId);
            }
            set { _requestId = value; }
        }

        #endregion
    }
}
