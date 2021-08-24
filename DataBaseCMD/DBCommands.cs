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
        public string SELECTUserAndPassword { get; set; }
        public string SELECTEmail { get; set; }
        public string SELECTUserName { get; set; }
        public string SELECTUser { get; set; }
        public string INSERTClient { get; set; }
        public string UpdatePicture { get; set; }
        private UserCredentials _userCredentials;

        public DBCommands(UserCredentials cred)
        {
            _userCredentials = cred;
            SELECTUserAndPassword = $"SELECT username, password FROM clients WHERE username='{_userCredentials.UserName}'" +
            $" AND password='{_userCredentials.DecryptedPassword}'";

            SELECTEmail = $"SELECT email FROM clients WHERE email='{_userCredentials.Email}'";

            SELECTUserName = $"SELECT username FROM clients WHERE username='{_userCredentials.UserName}'";

            SELECTUser = $"SELECT displayname, profilepicture FROM clients WHERE username='{_userCredentials.UserName}'" +
                $" AND password='{_userCredentials.DecryptedPassword}'";

            INSERTClient = $"INSERT INTO clients (client_id,username,password,displayname,email) VALUE(Default, '{_userCredentials.UserName}', '{_userCredentials.DecryptedPassword}', " +
                $"'{_userCredentials.DisplayName}', '{_userCredentials.Email}')";
        }
        public DBCommands(UserModel userModel) 
        {
            UpdatePicture = $"UPDATE clients SET profilepicture = '{userModel.ProfilePicture}' WHERE displayname = '{userModel.DisplayName}' ";
        }
 
        public bool UserExists(string Command)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                connection.Open();
                MySqlCommand command = new MySqlCommand(Command, connection);
                var user = command.ExecuteReader();    
                
                if (user.HasRows)
                    return true;
            }
            return false;
        }

        public UserModel GetUser(string Command)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                connection.Open();
                var user = connection.Query<UserModel>(Command).AsList();
                return user[0];
            }
        }

        public void ExecuteQuery(string Command)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                connection.Open();
                connection.Execute(Command);
            }
        }

     }
}
