using MassTransit;
using RustRetail.EmailSenderWorker.Services.Abstractions.Email;
using RustRetail.SharedContracts.IntegrationEvents.V1.NotificationService.Email;

namespace RustRetail.EmailSenderWorker.Consumers
{
    internal class EmailSendRequestedConsumer(
        IEmailSender emailSender,
        ILogger<EmailSendRequestedConsumer> logger,
        IPublishEndpoint publishEndpoint)
        : IConsumer<EmailSendRequestedEvent>
    {
        public async Task Consume(ConsumeContext<EmailSendRequestedEvent> context)
        {
            using var scope = logger.BeginScope(new Dictionary<string, object>
            {
                ["Scope"] = nameof(EmailSendRequestedConsumer), 
                ["NotificationId"] = context.Message.NotificationId,
                ["EmailTo"] = context.Message.To
            });

            try
            {
                await emailSender.SendEmailAsync([context.Message.To],
                    context.Message.Subject,
                    context.Message.Body,
                    cancellationToken: context.CancellationToken);

                await publishEndpoint.Publish(new EmailSentSuccessfullyEvent(context.Message.NotificationId), context.CancellationToken);
                logger.LogInformation("Email sent successfully to {Email} for notification {NotificationId}",
                    context.Message.To, context.Message.NotificationId);
            }
            catch (OperationCanceledException)
            {
                logger.LogInformation("Email sending was cancelled for notification {NotificationId}", context.Message.NotificationId);
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send email to {Email} for notification {NotificationId}",
                        context.Message.To, context.Message.NotificationId);
                throw;
            }
        }
    }

    // Configure MassTransit retry policy in service configuration
    public static class EmailSendRequestedConsumerConfiguration
    {
        public static void ConfigureEmailConsumer(this IRabbitMqBusFactoryConfigurator configurator,
            IBusRegistrationContext context)
        {
            configurator.ReceiveEndpoint("email-send-requested", e =>
            {
                e.ConfigureConsumer<EmailSendRequestedConsumer>(context);

                // Retry 3 times with exponential back-off
                e.UseMessageRetry(r => r.Exponential(
                    retryLimit: 3,
                    minInterval: TimeSpan.FromSeconds(2),
                    maxInterval: TimeSpan.FromSeconds(30),
                    intervalDelta: TimeSpan.FromSeconds(2)));
            });
        }
    }
}
