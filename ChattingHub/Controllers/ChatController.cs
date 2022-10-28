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
using System.Net.Http.Headers;
using Newtonsoft.Json;
using ChattingHub.Services;
using ChattingHub.Helper.Exceptions;

namespace ChattingHub.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private ILogger<ChatController> _logger;
        private static ChatHub _chathub = new ChatHub();
        private static DBCommands _dBCommands = new DBCommands();
        private IHubContext<ChatHub> _hubContext;
        private EmailService _emailService;
        public ChatController(ILogger<ChatController> logger, IHubContext<ChatHub> hubContext, EmailService emailService)
        {
            _logger = logger;
            _hubContext = hubContext;
            _emailService = emailService;
        }

        [HttpPost]
        [Route("Login")]
        public UserResponseModel Login(UserCredentials cred)
        {
            var credentialsExist = _dBCommands.CredentialsExist(cred);
            if (!credentialsExist)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return new UserResponseModel("User or Password incorrect.");
            }
            else
            {
                var user = _dBCommands.GetUser(cred.UserName);
                ChatHub.Data.Users.Add(user);
                _logger.LogInformation($"User {user.DisplayName} has logged in to the server.");
                Response.StatusCode = (int)HttpStatusCode.OK;
                return new UserResponseModel("Login was successful", user);
            }
        }

        [HttpPost]
        [Route("PostUser")]
        public string PostUser(UserCredentials cred)
        {
            var emailExists = _dBCommands.EmailExists(cred.Email);
            var userNameExists = _dBCommands.UserNameExists(cred.UserName);
            var DisplayNameExists = _dBCommands.DisplayNameExists(cred.DisplayName);
            if (emailExists)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "User with that Email Address already exists";
            }
            else if (userNameExists)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "UserName already exists";
            }
            else if (DisplayNameExists)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "User with that DisplayName already exists";
            }
            _dBCommands.InsertClient(cred);
            _logger.LogInformation($"User has registered an account." +
                $"\n            Username: {cred.UserName}" +
                $"\n            Password: {cred.DecryptedPassword}");
            Response.StatusCode = (int)HttpStatusCode.Accepted;
            return "User Registered";
        }

        [HttpPost]
        [Route("PostMessage")]
        public void AddMessage(MessageModel message)
        {
            _chathub.AddMessageData(message, _hubContext);
        }

        [HttpDelete]
        [Route("DeleteMessage")]
        public void DeleteMessage(MessageModel message)
        {
            _dBCommands.DeleteMessage(message);
            var messageToDelete = ChatHub.Data.Messages.First(x => x.MessageDate == message.MessageDate);
            ChatHub.Data.Messages.Remove(messageToDelete);
            _chathub.DeleteMessage(_hubContext, message);
        }

        [HttpPost]
        [Route("PostImage")]
        public async Task<string> UploadImage(ProfileImageDataModel imageUploadDataModel)
        {
            using (var httpclient = new HttpClient())
            {
                httpclient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Client-ID", "");
                httpclient.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "text/plain");
                var response = await httpclient.PostAsync("https://api.imgur.com/3/Image", new StringContent($"{imageUploadDataModel.Base64ImageData}"));
                var stringcontent = await response.Content.ReadAsStringAsync();
                var ImgurResponseModel = JsonConvert.DeserializeObject<ImgurResponseModel>(stringcontent);
                ChangeProfilePicture(new ImageUploaderModel(imageUploadDataModel.Uploader, ImgurResponseModel.Data.Link));
                return ImgurResponseModel.Data.Link;
            }
        } 

        [HttpPost]
        [Route("PostName")]
        public string UpdateName(NameChangeModel nameChangeModel)
        {
            if (_dBCommands.DisplayNameExists(nameChangeModel.NewName))
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return "User with that DisplayName already exists";
            }
            _dBCommands.UpdateDisplayName(nameChangeModel);
            _chathub.UpdateName(nameChangeModel, _hubContext);
            return "Name Successfully changed";
        }

        [HttpPost]
        [Route("PostEmail")]
        public async Task<string> SendEmail([FromBody] string email)
        {
            if (!_dBCommands.EmailExists(email))
            {
                Response.StatusCode = 404;
                return "Email not found!";
            }
            await _emailService.SendEmail(email);
            return "Email Sent!";
        }


        [HttpPost]
        [Route("PostMessagesInterval")]
        public void SaveMessagesInterval(UnLoadedMessagesIntervalModel unLoadedMessagesInterval)
        {
            ChatHub.Data.UnLoadedMessagesIntervalModels.Add(unLoadedMessagesInterval);
            _chathub.ReduceMessages();
            _dBCommands.InsertInterval(unLoadedMessagesInterval);
        }


        [HttpPost]
        [Route("PostPassword")]
        public string UpdatePassword(PasswordChangeModel passwordChangeModel)
        {
            try
            {
                _emailService.VerifyCode(passwordChangeModel);
            }
            catch (VerificationException e)
            {
                Response.StatusCode = (int)HttpStatusCode.NotFound;
                return e.Message;
            }
            _dBCommands.UpdatePassword(passwordChangeModel);
            return "Password Changed!";
        }

        private void ChangeProfilePicture(ImageUploaderModel profileImageUploadDataModel)
        {
            _dBCommands.UpdateProfilePicture(profileImageUploadDataModel);
            _chathub.UpdateImage(profileImageUploadDataModel, _hubContext);
        }
        [HttpGet]
        [Route("GetHeartBeat")]
        public string GetHeartBeat()
        {
            return "Alive";
        }
    }
}
