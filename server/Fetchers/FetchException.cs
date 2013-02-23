using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace bude.server
{
    public class FetchException : ApplicationException
    {
        public bool IsSecurityCodeRequested { get; set; }
        public bool IsSecurityQuestionAnswerNotProvided { get; set; }
        public string Message { get; set; }

        public FetchException(string message)
        {
            this.Message = message;
        }
    }
}
