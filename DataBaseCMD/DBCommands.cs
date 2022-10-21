using System;
using System.Collections.Generic;
using System.Text;
using Models;
using DataBaseCMD.Services;
using Dapper;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices;
using System.Data.Common;

namespace DataBaseCMD
{
    public sealed class DBCommands
    {
        private const string _connectionString = "";
        private string SELECTPasswordStatement => $"SELECT hashedpassword FROM clients WHERE username = @UserName";
        private string SELECTEmailStatement => $"SELECT email FROM clients WHERE email = @Email";
        private string SELECTUserNameStatement => $"SELECT username FROM clients WHERE username = @UserName";
        private string SELECTDisplayNametatement => $"SELECT displayname FROM clients WHERE displayname = @DisplayName";
        private string SELECTUserStatement => $"SELECT displayname, profilepicture FROM clients WHERE username = @UserName";
        private string INSERTClientStatement => $"INSERT INTO clients (client_id,username,hashedpassword,displayname,email,profilepicture) VALUE(Default, @UserName, @Password, @DisplayName, @Email, @ProfilePicture)";
        private string UpdatePictureStatement => $"UPDATE clients SET profilepicture = @ProfilePicture WHERE displayname = @DisplayName ";
        private string UpdateNameStatement => "UPDATE clients SET displayname = @NewDisplayName WHERE displayname = @CurrentDisplayName";
        private string UpdatePasswordStatement => "UPDATE clients SET hashedpassword = @NmewHashedPassword WHERE email = @Email";
        private string InsertMessageStatement => "Insert into messages VALUE((SELECT username from clients WHERE displayname = @Sender), " +
            "(SELECT username from clients WHERE displayname = @Receiver), @Message, @Date)";
        private string DeleteMessageStatement => "DELETE from messages where date = @MessageDate";
        private string InsertMessageIntervalsStatement => "Insert into messageIntervals VALUE(@FirstInterval, @LastInterval)";
        private string GetPublicMessagesAfterLastIntervalStatement => "SELECT * FROM messages where date > (SELECT LastInterval FROM clientinformation.messageintervals order by LastInterval desc limit 1)";
        private string GetPublicMessagesStatement => "SELECT * FROM messages where receiver is null";
        private string GetPublicMessagesIntervalStatement => "SELECT * FROM messages where date between @FirstInterval and @LastInterval";

        private string GetFirst5PublicIntervalsStatement => "SELECT * FROM messageintervals as MI where" +
            "(SELECT date from messages where date between MI.FirstInterval and MI.LastInterval and receiver is null limit 1) limit 5";


        public bool CredentialsExist(UserCredentials userCredentials)
        {

            var parameters = new { UserName = userCredentials.UserName };
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var hashedPassword = connection.ExecuteScalar(SELECTPasswordStatement, parameters);
                return hashedPassword == null ? false : EncryptionService.VerifyPassword(userCredentials.DecryptedPassword, hashedPassword as string);
            }
        }

        public bool EmailExists(string email)
        {
            var parameters = new { Email = email };
            using (var connection = new MySqlConnection(_connectionString))
            {
                using (var user = connection.ExecuteReader(SELECTEmailStatement, parameters))
                    return user.Read();
            }
        }
        public bool UserNameExists(string userName)
        {
            var parameters = new { UserName = userName };
            using (var connection = new MySqlConnection(_connectionString))
            {
                using (var user = connection.ExecuteReader(SELECTUserNameStatement, parameters))
                    return user.Read();
            }
        }
        public bool DisplayNameExists(string displayName)
        {
            var parameters = new { DisplayName = displayName };
            using (var connection = new MySqlConnection(_connectionString))
            {
                using (var user = connection.ExecuteReader(SELECTDisplayNametatement, parameters))
                    return user.Read();
            }
        }

        public UserModel GetUser(string userName)
        {
            var parameters = new { UserName = userName };
            using (var connection = new MySqlConnection(_connectionString))
            {
                var user = connection.QueryFirst<UserModel>(SELECTUserStatement, parameters);
                return user;
            }
        }

        public void UpdateProfilePicture(ImageUploaderModel profileImageDataModel)
        {
            var parameters = new { DisplayName = profileImageDataModel.Uploader.DisplayName, ProfilePicture = profileImageDataModel.Link };
            using (var connection = new MySqlConnection(_connectionString))
                connection.Execute(UpdatePictureStatement, parameters);

        }
        public void UpdateDisplayName(NameChangeModel nameChangeModel)
        {
            var parameters = new { NewDisplayName = nameChangeModel.NewName, CurrentDisplayName = nameChangeModel.User.DisplayName };
            using (var connection = new MySqlConnection(_connectionString))
                connection.Execute(UpdateNameStatement, parameters);
        }
        public void UpdatePassword(PasswordChangeModel passwordChangeModel)
        {
            var parameters2 = new { NewHashedPassword = EncryptionService.HashPassword(passwordChangeModel.Password), Email = passwordChangeModel.Email };
            using (var connection = new MySqlConnection(_connectionString))
                connection.Execute(UpdatePasswordStatement, parameters2);
        }

        public void InsertClient(UserCredentials userCredentials)
        {
            var parameters = new
            {
                UserName = userCredentials.UserName,
                Password = EncryptionService.HashPassword(userCredentials.DecryptedPassword),
                DisplayName = userCredentials.DisplayName,
                Email = userCredentials.Email,
                ProfilePicture = "https://imgur.com/Jn29VzN.jpg"
            };
            using (var connection = new MySqlConnection(_connectionString))
                connection.Execute(INSERTClientStatement, parameters);

        }

        public void InsertMessage(MessageModel message)
        {
            var parameters = new
            {
                Message = message.RTFData,
                Date = message.MessageDate,
                Sender = message.Sender.DisplayName,
                Receiver = message.DestinationUser?.DisplayName ?? null,
            };
            using (var connection = new MySqlConnection(_connectionString))
                connection.Execute(InsertMessageStatement, parameters);
        }

        public void DeleteMessage(MessageModel message)
        {
            var parameters = new
            {
                MessageDate = message.MessageDate,
            };
            using (var connection = new MySqlConnection(_connectionString))
                connection.Execute(DeleteMessageStatement, parameters);
        }

        public void InsertInterval(UnLoadedMessagesIntervalModel message)
        {
            var parameters = new
            {
                FirstInterval = message.FirstDate,
                LastInterval = message.LastDate,
            };
            using (var connection = new MySqlConnection(_connectionString))
                connection.Execute(InsertMessageIntervalsStatement, parameters);
        }

        public List<MessageModel> GetPublicMessages()
        {
            List<MessageModel> messages = new List<MessageModel>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                var data = connection.ExecuteReader(GetPublicMessagesAfterLastIntervalStatement);
                if (!data.Read())
                {
                    data.Dispose();
                    data = connection.ExecuteReader(GetPublicMessagesStatement);
                }
                while (data.Read())
                {
                    UserModel sender = GetUser(data.GetString(0));
                    byte[] messageData = data["message"] as byte[];
                    DateTime messageDate = (DateTime)data["date"];
                    messages.Add(new MessageModel(messageData, sender, null, messageDate));
                }

            }
            return messages;
        }

        public List<MessageModel> GetMessagesAfterInterval(UnLoadedMessagesIntervalModel unLoadedMessagesIntervalModel)
        {
            List<MessageModel> messages = new List<MessageModel>();
            var paramteres = new { FirstInterval = unLoadedMessagesIntervalModel.FirstDate, LastInterval = unLoadedMessagesIntervalModel.LastDate };
            using (var connection = new MySqlConnection(_connectionString))
            {
                using (var data = connection.ExecuteReader(GetPublicMessagesIntervalStatement, paramteres))
                {
                    while (data.Read())
                    {
                        UserModel sender = GetUser(data.GetString(0));
                        byte[] messageData = data["message"] as byte[];
                        DateTime messageDate = (DateTime)data["date"];
                        messages.Add(new MessageModel(messageData, sender, null, messageDate));
                    }
                }

            }
            return messages;
        }

        public List<UnLoadedMessagesIntervalModel> GetFirst5PublicIntervals()
        {
            List<UnLoadedMessagesIntervalModel> intervals = new List<UnLoadedMessagesIntervalModel>();
            using (var connection = new MySqlConnection(_connectionString))
            {
                using (var data = connection.ExecuteReader(GetFirst5PublicIntervalsStatement))
                {
                    while (data.Read())
                    {
                        DateTime firstInterval = (DateTime)data["FirstInterval"];
                        DateTime LastInterval = (DateTime)data["LastInterval"];
                        intervals.Add(new UnLoadedMessagesIntervalModel(firstInterval, LastInterval));
                    }
                }

            }
            return intervals;
        }

    }
}
