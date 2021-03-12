using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Models
{
    public class UserResponseModel
    {
        public HttpStatusCode ResponseCode { get; set; } 
        public string Message { get; set; } 
        public UserModel Payload { get; set; }
    }
}
