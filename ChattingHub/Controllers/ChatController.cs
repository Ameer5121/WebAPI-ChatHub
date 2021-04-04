using ChattingHub.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Models;
using DataBaseCMD;
using System.Net.Http;
using System.Net;

namespace ChattingHub.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private ILogger<ChatController> _logger;
        private ChatHub _chathub;
        private IHubContext<ChatHub> _hubContext;
        private DBCommands dBCommands;
        public ChatController(ILogger<ChatController> logger, IHubContext<ChatHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
            _chathub = new ChatHub();
        }

        [HttpPost]
        [Route("Login")]
        public UserResponseModel Login(UserCredentials cred)
        {
            dBCommands = new DBCommands(cred);
            var userExists = dBCommands.UserExists(dBCommands.SELECTUserAndPassword);
            if (!userExists)
            {
                return new UserResponseModel
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    Message = "User or Password incorrect.",
                };
            }
            else
            {
                var user = dBCommands.GetUser(dBCommands.SELECTDisplayName);
                _chathub.AddUserData(user);
                _logger.LogInformation($"User {user.DisplayName} has logged in to the server.");
                return new UserResponseModel
                {
                    ResponseCode = HttpStatusCode.OK,
                    Message = "Login was successful",
                    Payload = user
                };
            }    
        }

        [HttpPost]
        [Route("PostUser")]
        public UserResponseModel PostUser(UserCredentials cred)
        {
            dBCommands = new DBCommands(cred);
            var userWithEmailExists = dBCommands.UserExists(dBCommands.SELECTEmail);
            var userNameExists = dBCommands.UserExists(dBCommands.SELECTUser);
            if (userWithEmailExists)
            {
                return new UserResponseModel
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    Message = "User with that Email Address already exists",
                };
            }
            else if (userNameExists)
            {
                return new UserResponseModel
                {
                    ResponseCode = HttpStatusCode.BadRequest,
                    Message = "UserName already exists",
                };
            }

            dBCommands.RegisterUser(dBCommands.INSERTClient);
            _logger.LogInformation($"User has registered an account." +
                $"\n            Username: {cred.UserName}" +
                $"\n            Password: {cred.DecryptedPassword}");
            return new UserResponseModel
            {
                ResponseCode = HttpStatusCode.Accepted,
            };
        }

        [HttpPost]
        [Route("PostMessage")]
        public void AddMessage(MessageModel message)
        {
            _chathub.AddMessageData(message, _hubContext);
        }

        [HttpGet]
        [Route("GetHeartBeat")]
        public string GetHeartBeat()
        {
            return "Alive";
        }
    }
}
