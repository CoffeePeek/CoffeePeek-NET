using CoffeePeek.ShopsService;

var builder = WebApplication.CreateBuilder(args);

builder.AddApplication();

var app = builder.Build();

await app.UseApplication();

app.Run();
