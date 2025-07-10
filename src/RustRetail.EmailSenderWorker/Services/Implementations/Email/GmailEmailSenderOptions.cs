namespace RustRetail.EmailSenderWorker.Services.Implementations.Email
{
    internal class GmailEmailSenderOptions
    {
        public const string SectionName = nameof(GmailEmailSenderOptions);

        public string FromEmail { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; } = 587;
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
