using System;
using BC = BCrypt.Net.BCrypt;
using System.Collections.Generic;
using System.Text;
using Models;

namespace DataBaseCMD.Services
{
    public static class EncryptionService
    {
        public static string HashPassword(UserCredentials credentials)
        {
           return BC.HashPassword(credentials.DecryptedPassword);           
        }

        public static bool VerifyPassword(UserCredentials credentials, string hashedPassword)
        {
            return BC.Verify(credentials.DecryptedPassword, hashedPassword);
        }
    }
}
