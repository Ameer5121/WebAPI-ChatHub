﻿using ChattingHub.Hubs;
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
using System.Net.Http.Headers;
using Newtonsoft.Json;

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
            var credentialsExist = dBCommands.CredentialsExist();
            if (!credentialsExist)
            {
                return new UserResponseModel
                {
                    ResponseCode = HttpStatusCode.NotFound,
                    Message = "User or Password incorrect.",
                };
            }
            else
            {
                var user = dBCommands.GetUser();
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
            var emailExists = dBCommands.EmailExists();
            var userNameExists = dBCommands.UserNameExists();
            if (emailExists)
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

            dBCommands.InsertClient();
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

        [HttpPost]
        [Route("PostImage")]
        public async Task<string> UploadImage(ProfileImageDataModel profileImageDataModel)
        {
            var httpclient = new HttpClient();
            httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", "");
            httpclient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "text/plain");
            var response = await httpclient.PostAsync("https://api.imgur.com/3/Image", new StringContent($"{profileImageDataModel.Base64ImageData}"));           
            var stringcontent = await response.Content.ReadAsStringAsync();
            var ImgurResponseModel = JsonConvert.DeserializeObject<ImgurResponseModel>(stringcontent);
            ChangeProfilePicture(ImgurResponseModel, profileImageDataModel);
            return ImgurResponseModel.Data.Link;
        }

        private void ChangeProfilePicture(ImgurResponseModel imgurResponseModel, ProfileImageDataModel profileImageDataModel)
        {
            var userModel = _chathub.UpdateImage(imgurResponseModel.Data.Link, profileImageDataModel.Uploader, _hubContext);
            dBCommands = new DBCommands(userModel);
            dBCommands.UpdateProfilePicture();
        }
        [HttpGet]
        [Route("GetHeartBeat")]
        public string GetHeartBeat()
        {
            return "Alive";
        }
    }
}
