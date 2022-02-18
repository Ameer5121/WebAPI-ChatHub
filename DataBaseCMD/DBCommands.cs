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
        private string SELECTUserStatement => $"SELECT displayname, profilepicture FROM clients WHERE username = @UserName";
        private string INSERTClientStatement => $"INSERT INTO clients (client_id,username,hashedpassword,displayname,email,profilepicture) VALUE(Default, @UserName, @Password, @DisplayName, @Email, @ProfilePicture)";

        private string UpdatePictureStatement => $"UPDATE clients SET profilepicture = @ProfilePicture WHERE displayname = @DisplayName ";
        private string UpdateNameStatement => "UPDATE clients SET displayname = @NewDisplayName WHERE displayname = @CurrentDisplayName";
        private string UpdatePasswordStatement => "UPDATE clients SET hashedpassword = @NewHashedPassword WHERE email = @Email";


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

        public UserModel GetUser(string userName)
        {
            var parameters = new { UserName = userName };
            using (var connection = new MySqlConnection(_connectionString))
            {
                var user = connection.Query<UserModel>(SELECTUserStatement, parameters).AsList();
                return user[0];
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

    }
}
