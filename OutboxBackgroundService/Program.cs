using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Infrastructure.Options;
using MassTransit;
using OutboxBackgroundService;
using OutboxBackgroundService.Configuration;
using CoffeePeek.Shared.Extensions.Logging;
using CoffePeek.ServiceDefaults;
using Microsoft.AspNetCore.Builder;
using PostgresCpOptions = OutboxBackgroundService.PostgresCpOptions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSerilogLogging();

builder.Services.AddHostedService<Worker>();

var postgresCpOptions = builder.Services.AddValidateOptions<PostgresCpOptions>();

builder.Services.RegisterOutboxDbContexts(postgresCpOptions);

var rabbitMqOptions = builder.Services.AddValidateOptions<RabbitMqOptions>();
var railwayRabbitMqOptions = RabbitMqConnectionHelper.GetRabbitMqOptions();
if (railwayRabbitMqOptions != null)
{
    rabbitMqOptions.HostName = railwayRabbitMqOptions.HostName;
    rabbitMqOptions.Port = railwayRabbitMqOptions.Port;
    rabbitMqOptions.Username = railwayRabbitMqOptions.Username;
    rabbitMqOptions.Password = railwayRabbitMqOptions.Password;
}

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqOptions.HostName, rabbitMqOptions.Port, "/", h =>
        {
            h.Username(rabbitMqOptions.Username);
            h.Password(rabbitMqOptions.Password);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();