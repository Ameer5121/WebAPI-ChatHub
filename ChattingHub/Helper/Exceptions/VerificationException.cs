using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChattingHub.Helper.Exceptions
{
    public class VerificationException : Exception
    {
        public VerificationException(string message) : base(message)
        {
        }
    }
}
