using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ChattingHub.Services
{
    public class EmailService
    {
        private Dictionary<int, string> _recoveryEmails;
        private SmtpClient _smtpClient;
        public EmailService()
        {
            _recoveryEmails = new Dictionary<int, string>();
            _smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential("EMAIL", "PASSWORD"),
                EnableSsl = true,
            };
        }
        public async Task SendEmail(string receipent)
        {
            var codeGenerator = new Random();
            var code = codeGenerator.Next(100000, 999999);
            MailAddress from = new MailAddress("EMAIL");
            MailAddress to = new MailAddress(receipent);
            MailMessage message = new MailMessage(from, to);
            message.Subject = "Verification Code";
            message.Body = $"Your verification code is {code}";
            await _smtpClient.SendMailAsync(message);
            SaveEmail(receipent, code);
        }
        private void SaveEmail(string receipent, int code)
        {
            if (_recoveryEmails.ContainsValue(receipent)) foreach (var value in _recoveryEmails.Where(x => x.Value == receipent)) _recoveryEmails.Remove(value.Key);
            _recoveryEmails.Add(code, receipent);
        }
 
    }
}
