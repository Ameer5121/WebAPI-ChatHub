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
        private const string _connection = "";
        private string SELECTPassword => $"SELECT hashedpassword FROM clients WHERE username = @UserName";
        private string SELECTEmail => $"SELECT email FROM clients WHERE email = @Email";
        private string SELECTUserName => $"SELECT username FROM clients WHERE username = @UserName";
        private string SELECTUser => $"SELECT displayname, profilepicture FROM clients WHERE username = @UserName";
        private string INSERTClient => $"INSERT INTO clients (client_id,username,hashedpassword,displayname,email,profilepicture) VALUE(Default, @UserName, @Password, @DisplayName, @Email, @ProfilePicture)";

        private string UpdatePicture => $"UPDATE clients SET profilepicture = @ProfilePicture WHERE displayname = @DisplayName ";
        private string UpdateName => "UPDATE clients SET displayname = @NewDisplayName WHERE displayname = @CurrentDisplayName";


        public bool CredentialsExist(UserCredentials userCredentials)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { UserName = userCredentials.UserName};
                connection.Open();
                var hashedPassword = connection.ExecuteScalar(SELECTPassword, parameters);
                return hashedPassword == null ? false : EncryptionService.VerifyPassword(userCredentials.DecryptedPassword, hashedPassword as string);                
            }
        }

        public bool EmailExists(string email)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { Email = email };
                connection.Open();
                var user = connection.ExecuteReader(SELECTEmail, parameters);
               return user.Read();
            }
        }
        public bool UserNameExists(string userName)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { UserName = userName };
                connection.Open();
                var user = connection.ExecuteReader(SELECTUserName, parameters);
                return user.Read();
            }
        }

        public UserModel GetUser(string userName)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { UserName = userName };              
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
                var parameters = new { UserName = userCredentials.UserName, Password = EncryptionService.HashPassword(userCredentials.DecryptedPassword), DisplayName = userCredentials.DisplayName, 
                    Email = userCredentials.Email, ProfilePicture = "https://imgur.com/Jn29VzN.jpg"};
                connection.Open();
                connection.Execute(INSERTClient, parameters);
            }
        }

    }
}
