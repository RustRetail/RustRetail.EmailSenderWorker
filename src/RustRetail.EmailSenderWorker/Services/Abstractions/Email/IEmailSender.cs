namespace RustRetail.EmailSenderWorker.Services.Abstractions.Email
{
    internal interface IEmailSender
    {
        Task SendEmailAsync(
            IEnumerable<string> recipients,
            string subject,
            string body,
            IEnumerable<string>? ccRecipients = default,
            IEnumerable<string>? bccRecipients = default,
            CancellationToken cancellationToken = default);
    }
}
