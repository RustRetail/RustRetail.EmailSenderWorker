using MassTransit;
using RustRetail.SharedContracts.IntegrationEvents.V1.NotificationService.Email;

namespace RustRetail.EmailSenderWorker.Consumers
{
    internal class EmailSendRequestedFailedConsumer(
        ILogger<EmailSendRequestedFailedConsumer> logger,
        IPublishEndpoint publishEndpoint)
        : IConsumer<Fault<EmailSendRequestedEvent>>
    {
        public async Task Consume(ConsumeContext<Fault<EmailSendRequestedEvent>> context)
        {
            var originalMessage = context.Message.Message;
            var exceptionMessage = context.Message.Exceptions.FirstOrDefault()?.Message ?? "Unknown error";
            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["Scope"] = nameof(EmailSendRequestedFailedConsumer),
                ["NotificationId"] = originalMessage.NotificationId,
                ["EmailTo"] = originalMessage.To
            });
            logger.LogWarning("All retry attempts failed for sending email to {Email}. Publishing failure event.",
                originalMessage.To);

            await publishEndpoint.Publish(
                new EmailSentFailedEvent(originalMessage.NotificationId, exceptionMessage),
                context.CancellationToken);
        }
    }
}
