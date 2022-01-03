using System;
using BC = BCrypt.Net.BCrypt;
using System.Collections.Generic;
using System.Text;
using Models;

namespace DataBaseCMD.Services
{
    public static class EncryptionService
    {
        public static string HashPassword(string password) =>  BC.HashPassword(password);   
        public static bool VerifyPassword(string decryptedPassword, string hashedPassword) => BC.Verify(decryptedPassword, hashedPassword);
    }
}
