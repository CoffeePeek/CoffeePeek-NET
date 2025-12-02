using CoffeePeek.AuthService.Configuration;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Options;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using OutboxBackgroundService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();
var dbOptions = builder.Services.AddValidateOptions<PostgresCpOptions>();
builder.Services.AddDbContext<AuthDbContext>(opt => { opt.UseNpgsql(dbOptions.ConnectionString); });

builder.Services.AddValidateOptions<RabbitMqOptions>();
var rabbitMqOptions = builder.Services.GetOptions<RabbitMqOptions>();
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