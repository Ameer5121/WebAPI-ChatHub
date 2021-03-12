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
        private const string _connection = "Server = 192.168.14.15; Database = clientinformation; Uid = DBC; Pwd = 123321soundisound";
        public static string SELECTUserAndPassword;
        public static string SELECTEmail;
        public static string SELECTDisplayName;
        public static string INSERTClient;
        private UserCredentials _userCredentials; 
        public DBCommands(UserCredentials cred)
        {
            _userCredentials = cred;
            SELECTUserAndPassword = $"SELECT username, password FROM clients WHERE username='{_userCredentials.UserName}'" +
                $" AND password='{_userCredentials.DecryptedPassword}'";

            SELECTEmail = $"SELECT email FROM clients WHERE email='{_userCredentials.Email}'";

            SELECTDisplayName = $"SELECT displayname FROM clients WHERE username='{_userCredentials.UserName}'" +
                $" AND password='{_userCredentials.DecryptedPassword}'";

            INSERTClient = $"INSERT INTO clients VALUE(Default, '{cred.UserName}', '{cred.DecryptedPassword}', " +
                $"'{cred.DisplayName}', '{cred.Email}')";
        }
        public bool FindUser(string Command)
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

        public void RegisterUser(string Command)
        {
            using (MySqlConnection connection = new MySqlConnection(_connection))
            {
                connection.Open();
                connection.Execute(Command);
            }
        }

     }
}
