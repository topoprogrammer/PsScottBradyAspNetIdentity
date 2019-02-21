using Microsoft.AspNet.Identity;
using System.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;

namespace WebAspNetIdentity.Services
{
    public class EmailService : IIdentityMessageService
    {
        private readonly string _host;
        private readonly int _port;
        private readonly string _userName;
        private readonly string _password;

        public EmailService()
        {
            _host = ConfigurationManager.AppSettings["smtpClient:Host"];
            _port = int.Parse(ConfigurationManager.AppSettings["smtpClient:Port"]);
            _userName = ConfigurationManager.AppSettings["smtpClient:UserName"];
            _password = ConfigurationManager.AppSettings["smtpClient:Password"];
        }

        public Task SendAsync(IdentityMessage message)
        {
            return SendMail(message);
        }

        private async Task SendMail(IdentityMessage message)
        {
            using (var smtpClient = new SmtpClient(_host))
            {
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Host = _host;
                smtpClient.Port = _port;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials = new System.Net.NetworkCredential(_userName, _password);
                var mailMessage = CreateDefaultMessage(message);
                //mailMessage.To.Add(message.Destination);
                mailMessage.To.Add(_userName);
                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
                catch
                {
                    // ignored
                }
            }
        }
        private MailMessage CreateDefaultMessage(IdentityMessage message)
        {
            return new MailMessage
            {
                From = new MailAddress(_userName),
                Subject = message.Subject,
                Body = message.Body
            };
        }
    }
}
