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
        private string SELECTUserNameAndPassword;
        private string SELECTEmail;
        private string SELECTUserName;
        private string SELECTUser;
        private string INSERTClient;
        private string UpdatePicture;
        private UserCredentials _userCredentials;
        private UserModel _userModel;

        public DBCommands(UserCredentials cred)
        {
            _userCredentials = cred;
            SELECTUserNameAndPassword = $"SELECT username, password FROM clients WHERE username = @UserName AND password = @Password";

            SELECTEmail = $"SELECT email FROM clients WHERE email = @Email";

            SELECTUserName = $"SELECT username FROM clients WHERE username = @UserName";

            SELECTUser = $"SELECT displayname, profilepicture FROM clients WHERE username = @UserName" +
                $" AND password = @Password";

            INSERTClient = $"INSERT INTO clients (client_id,username,password,displayname,email) VALUE(Default, @UserName, @Password, @DisplayName, @Email)";
        }
        public DBCommands(UserModel userModel)
        {
            _userModel = userModel;
            UpdatePicture = $"UPDATE clients SET profilepicture = @ProfilePicture WHERE displayname = @DisplayName ";
        }

        public bool CredentialsExist()
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { UserName = _userCredentials.UserName, Password = _userCredentials.DecryptedPassword };
                connection.Open();
                var user = connection.ExecuteReader(SELECTUserNameAndPassword, parameters);
                return user.Read();
            }
        }

        public bool EmailExists()
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { Email = _userCredentials.Email };
                connection.Open();
                var user = connection.ExecuteReader(SELECTEmail, parameters) as MySqlDataReader;
                if (user.HasRows)
                    return true;
            }
            return false;
        }
        public bool UserNameExists()
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { UserName = _userCredentials.UserName };
                connection.Open();
                var user = connection.ExecuteReader(SELECTUserName, parameters) as MySqlDataReader;
                if (user.HasRows)
                    return true;
            }
            return false;
        }

        public UserModel GetUser()
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { UserName = _userCredentials.UserName, Password = _userCredentials.DecryptedPassword };              
                connection.Open();
                var user = connection.Query<UserModel>(SELECTUser, parameters).AsList();
                return user[0];
            }
        }

        public void UpdateProfilePicture()
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { DisplayName = _userModel.DisplayName, ProfilePicture = _userModel.ProfilePicture };
                connection.Open();
                connection.Execute(UpdatePicture, parameters);
            }
        }

        public void InsertClient()
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                var parameters = new { UserName = _userCredentials.UserName, Password = _userCredentials.DecryptedPassword, DisplayName = _userCredentials.DisplayName, Email = _userCredentials.Email };
                connection.Open();
                connection.Execute(INSERTClient, parameters);
            }
        }

    }
}
