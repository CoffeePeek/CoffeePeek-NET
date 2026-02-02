using CoffeePeek.Shared.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var accountService = builder
    .AddProject<Projects.CoffeePeek_AccountService>(AppResources.AccountService)
    .WithUrl("/swagger", "Swagger UI")
    .WithUrl("/cap", "Cap dashboard");

var jobVacanciesService = builder
    .AddProject<Projects.CoffeePeek_JobVacancies>(AppResources.JobVacanciesService)
    .WithUrl("/swagger", "Swagger UI")
    .WithUrl("/cap", "Cap dashboard");

var shopsService = builder
    .AddProject<Projects.CoffeePeek_ShopsService>(AppResources.ShopsService)
    .WithUrl("/swagger", "Swagger UI")
    .WithUrl("/cap", "Cap dashboard");

var moderationService = builder
    .AddProject<Projects.CoffeePeek_ModerationService>(AppResources.ModerationService)
    .WithUrl("/swagger", "Swagger UI")
    .WithUrl("/cap", "Cap dashboard");

var mediaService = builder
    .AddProject<Projects.CoffeePeek_MediaService>(AppResources.MediaService)
    .WithUrl("/swagger", "Swagger UI")
    .WithUrl("/cap", "Cap dashboard");

builder.AddProject<Projects.CoffeePeek_Gateway>(AppResources.Gateway)
    .WithReference(accountService)
    .WithReference(jobVacanciesService)
    .WithReference(shopsService)
    .WithReference(moderationService)
    .WithReference(mediaService)
    .WithEnvironment("DOTNET_ASPIRE", "true")
    .WithEnvironment("DOTNET_ASPIRE_RUNNING", "true")
    .WithUrl("/swagger", "Swagger UI");

builder.Build().Run();