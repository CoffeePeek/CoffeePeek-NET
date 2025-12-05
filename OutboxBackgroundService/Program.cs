using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Options;
using CoffeePeek.Shared.Infrastructure.Options;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OutboxBackgroundService;
using OutboxBackgroundService.Configuration;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
var dbOptions = builder.Services.AddValidateOptions<PostgresCpOptions>();
builder.Services.AddDbContext<AuthDbContext>(opt => { opt.UseNpgsql(dbOptions.ConnectionString); });

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


var host = builder.Build();
host.Run();