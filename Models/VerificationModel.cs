using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class VerificationModel
    {
        public int Code { get; }
        public string Email { get; }
        public VerificationModel(int code, string email)
        {
            Code = code;
            Email = email;
        }
    }
}
