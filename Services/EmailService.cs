﻿using Microsoft.Extensions.Options;
using react_app.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace react_app.Services
{
    public class EmailService
    {
        private readonly IOptions<EmailSettings> _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings;
        }

        public Task SendEmailAsync(string email, string subject, string message, string attachmentFilepath = null)
        {
            var mailMessage = new MailMessage(_emailSettings.Value.MailSender, email, subject, message);
            mailMessage.IsBodyHtml = true;

            if(attachmentFilepath != null)
            {
                var attachment = new Attachment(attachmentFilepath);
                mailMessage.Attachments.Add(attachment);
            }

            var client = new SmtpClient
            {
                Port = _emailSettings.Value.MailPort,
                Host = _emailSettings.Value.MailHost,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.Value.MailUsername, _emailSettings.Value.MailPassword)
            };
            client.SendCompleted += (s, e) => {
                client.Dispose();
                mailMessage.Dispose();
            };

            return client.SendMailAsync(mailMessage);
        }
    }
}
