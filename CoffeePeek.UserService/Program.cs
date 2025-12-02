using CoffeePeek.Contract.Events;
using CoffeePeek.Shared.Extensions.Configuration;
using CoffeePeek.Shared.Extensions.Options;
using CoffeePeek.UserService.Configuration;
using CoffeePeek.UserService.EventConsumer;
using CoffeePeek.UserService.Repositories;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddValidateOptions<RabbitMqOptions>();
        
var rabbitMqOptions = builder.Services.GetOptions<RabbitMqOptions>();

var dbOptions = builder.Services.AddValidateOptions<PostgresCpOptions>();
builder.Services.AddDbContext<UserDbContext>(opt => { opt.UseNpgsql(dbOptions.ConnectionString); });

builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<UserRegisteredEventConsumer>();
    
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();