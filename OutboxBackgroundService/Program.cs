using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Infrastructure.Options;
using MassTransit;
using OutboxBackgroundService;
using OutboxBackgroundService.Configuration;
using CoffeePeek.Shared.Extensions.Logging;
using CoffeePeek.Shared.Extensions.Modules;
using CoffePeek.ServiceDefaults;
using Microsoft.AspNetCore.Builder;
using PostgresCpOptions = OutboxBackgroundService.PostgresCpOptions;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddSerilogLogging();

builder.Services.AddHostedService<Worker>();

var postgresCpOptions = builder.Services.AddValidateOptions<PostgresCpOptions>();

builder.Services.RegisterOutboxDbContexts(postgresCpOptions);

builder.Services.AddMessagingModule();

var app = builder.Build();

app.MapDefaultEndpoints();

app.Run();