using CoffeePeek.Moderation.Application.Services;
using CoffeePeek.Moderation.BuildingBlocks.MediatR;
using CoffeePeek.Shared.Extensions.Hashing;

namespace CoffeePeek.Moderation.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers().AddNewtonsoftJson();

        ///Conf DI
        
        builder.Services.AddSingleton<IHashingService, HashingService>();

        builder.Services.AddScoped<ValidationStrategy>();
        
        ///Conf DI
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        
        
        builder.Services
            .ConfigureMediatR();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}