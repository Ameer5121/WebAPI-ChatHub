using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Models
{
    public class UserResponseModel
    {
        public string Message { get; } 
        public UserModel Payload { get;}

        public UserResponseModel(string message) => Message = message;
        public UserResponseModel(string message, UserModel payLoad)
        {
            Message = message;
            Payload = payLoad;
        }
    }
}
