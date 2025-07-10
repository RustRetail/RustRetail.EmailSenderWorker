using Microsoft.Extensions.Options;
using RustRetail.EmailSenderWorker.Services.Abstractions.Email;
using System.Net;
using System.Net.Mail;

namespace RustRetail.EmailSenderWorker.Services.Implementations.Email
{
    internal class GmailEmailSender : IEmailSender
    {
        readonly GmailEmailSenderOptions _options;
        readonly SmtpClient _smtpClient;
        readonly ILogger<GmailEmailSender> _logger;

        public GmailEmailSender(IOptions<GmailEmailSenderOptions> options,
            ILogger<GmailEmailSender> logger)
        {
            _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _smtpClient = new SmtpClient(_options.Host, _options.Port)
            {
                Credentials = new NetworkCredential(_options.UserName, _options.Password),
                EnableSsl = true
            };
        }

        public async Task SendEmailAsync(IEnumerable<string> recipients,
            string subject,
            string body,
            IEnumerable<string>? ccRecipients = null,
            IEnumerable<string>? bccRecipients = null,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(recipients);
            if (!recipients.Any())
            {
                throw new ArgumentException("At least one recipient must be provided.", nameof(recipients));
            }
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);
            ArgumentException.ThrowIfNullOrWhiteSpace(body);

            var mailMessage = GetMailMessage(subject, body);
            foreach (var recipient in recipients)
            {
                mailMessage.To.Add(new MailAddress(recipient));
            }
            if (ccRecipients != null)
            {
                foreach (var cc in ccRecipients)
                {
                    mailMessage.CC.Add(new MailAddress(cc));
                }
            }
            if (bccRecipients != null)
            {
                foreach (var bcc in bccRecipients)
                {
                    mailMessage.Bcc.Add(new MailAddress(bcc));
                }
            }
            try
            {
                await _smtpClient.SendMailAsync(mailMessage, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Recipients}", string.Join(", ", recipients));
            }
        }

        private MailMessage GetMailMessage(string subject, string body, bool isHtmlBody = true)
            => new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.DisplayName),
                Subject = subject,
                Body = body,
                IsBodyHtml = isHtmlBody,
            };
    }
}
