using MassTransit;
using RustRetail.EmailSenderWorker.Consumers;
using RustRetail.EmailSenderWorker.Services.Abstractions.Email;
using RustRetail.EmailSenderWorker.Services.Implementations.Email;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(x =>
{
    x.SetKebabCaseEndpointNameFormatter();
    x.AddConsumer<EmailSendRequestedConsumer>();
    x.AddConsumer<EmailSendRequestedFailedConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetValue<string>("RabbitMq:HostName"), builder.Configuration.GetValue<string>("RabbitMq:VirtualHost"), h =>
        {
            h.Username(builder.Configuration.GetValue<string>("RabbitMq:Username")!);
            h.Password(builder.Configuration.GetValue<string>("RabbitMq:Password")!);
        });
        cfg.ConfigureEmailConsumer(context);
        cfg.ConfigureEndpoints(context);
    });
});
builder.Services.Configure<GmailEmailSenderOptions>(builder.Configuration.GetSection(GmailEmailSenderOptions.SectionName));
builder.Services.AddScoped<IEmailSender, GmailEmailSender>();

var host = builder.Build();
host.Run();
