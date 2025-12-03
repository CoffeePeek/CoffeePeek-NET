using CoffeePeek.Photo.Api.Configuration;

namespace CoffeePeek.Photo.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        // Настройка для прослушивания на IPv6 (требуется для Railway приватной сети)
        // IPv6 слушает на всех интерфейсах, включая IPv4 через IPv6-mapped адреса
        builder.WebHost.UseUrls("http://[::]:80");
        
        builder.Services.AddAuthorization();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddControllers();
        
        builder.Services.ConfigureApi();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();
        
        Console.WriteLine("Listening on: http://[::]:80 (IPv6 for Railway private network)");
        
        app.Run();
    }
}