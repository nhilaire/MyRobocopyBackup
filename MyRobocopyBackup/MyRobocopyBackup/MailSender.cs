using System.Net;
using System.Net.Mail;

namespace MyRobocopyBackup
{
    internal class MailSender : IMailSender
    {
        private readonly MailSettingsConfiguration _mailSettings;

        public MailSender(MailSettingsConfiguration mailSettings)
        {
            _mailSettings = mailSettings;
        }

        public void SendMail(string subject, string content)
        {
            var fromAddress = new MailAddress(_mailSettings.From, _mailSettings.FromName);
            var toAddress = new MailAddress(_mailSettings.Dest, _mailSettings.Dest);

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, _mailSettings.SmtpPassword)
            };
            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = content,
                IsBodyHtml = true
            };
            smtp.Send(message);
        }
    }
}
