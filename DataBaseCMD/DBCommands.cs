using System;
using System.Collections.Generic;
using System.Text;
using Models;
using Dapper;
using MySql.Data.MySqlClient;
using System.Runtime.InteropServices;
using System.Data.Common;

namespace DataBaseCMD
{
    public sealed class DBCommands
    {
        private const string _connection = "";
        private string SELECTUserNameAndPassword => $"SELECT username, password FROM clients WHERE username = @UserName AND password = @Password";
        private string SELECTEmail => $"SELECT email FROM clients WHERE email = @Email";
        private string SELECTUserName => $"SELECT username FROM clients WHERE username = @UserName";
        private string SELECTUser => $"SELECT displayname, profilepicture FROM clients WHERE username = @UserName" +
                $" AND password = @Password";
        private string INSERTClient => $"INSERT INTO clients (client_id,username,password,displayname,email,profilepicture) VALUE(Default, @UserName, @Password, @DisplayName, @Email, @ProfilePicture)";

        private string UpdatePicture => $"UPDATE clients SET profilepicture = @ProfilePicture WHERE displayname = @DisplayName ";
        private string UpdateName => "UPDATE clients SET displayname = @NewDisplayName WHERE displayname = @CurrentDisplayName";


        public bool CredentialsExist(UserCredentials userCredentials)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { UserName = userCredentials.UserName, Password = userCredentials.DecryptedPassword };
                connection.Open();
                var user = connection.ExecuteReader(SELECTUserNameAndPassword, parameters);
                return user.Read();
            }
        }

        public bool EmailExists(UserCredentials userCredentials)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { Email = userCredentials.Email };
                connection.Open();
                var user = connection.ExecuteReader(SELECTEmail, parameters);
               return user.Read();
            }
        }
        public bool UserNameExists(UserCredentials userCredentials)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { UserName = userCredentials.UserName };
                connection.Open();
                var user = connection.ExecuteReader(SELECTUserName, parameters);
                return user.Read();
            }
        }

        public UserModel GetUser(UserCredentials userCredentials)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { UserName = userCredentials.UserName, Password = userCredentials.DecryptedPassword };              
                connection.Open();
                var user = connection.Query<UserModel>(SELECTUser, parameters).AsList();
                return user[0];
            }
        }

        public void UpdateProfilePicture(ProfileImageUploadDataModel profileImageDataModel)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { DisplayName = profileImageDataModel.Uploader.DisplayName, ProfilePicture = profileImageDataModel.Link };
                connection.Open();
                connection.Execute(UpdatePicture, parameters);
            }
        }
        public void UpdateDisplayName(NameChangeModel nameChangeModel)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { NewDisplayName = nameChangeModel.NewName, CurrentDisplayName = nameChangeModel.User.DisplayName };
                connection.Open();
                connection.Execute(UpdateName, parameters);
            }
        }

        public void InsertClient(UserCredentials userCredentials)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { UserName = userCredentials.UserName, Password = userCredentials.DecryptedPassword, DisplayName = userCredentials.DisplayName, 
                    Email = userCredentials.Email, ProfilePicture = "https://imgur.com/Jn29VzN.jpg"};
                connection.Open();
                connection.Execute(INSERTClient, parameters);
            }
        }

    }
}
