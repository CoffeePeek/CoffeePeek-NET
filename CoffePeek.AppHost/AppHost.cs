using CoffeePeek.Shared.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var authService = builder.AddProject<Projects.CoffeePeek_AuthService>(AppResources.AuthService);

var userService = builder.AddProject<Projects.CoffeePeek_UserService>(AppResources.UserService);

var jobVacanciesService = builder.AddProject<Projects.CoffeePeek_JobVacancies>(AppResources.JobVacanciesService);

var shopsService = builder.AddProject<Projects.CoffeePeek_ShopsService>(AppResources.ShopsService);

var moderationService = builder.AddProject<Projects.CoffeePeek_ModerationService>(AppResources.ModerationService);

var outboxService = builder.AddProject<Projects.OutboxBackgroundService>(AppResources.OutboxBackgroundService);

builder.AddProject<Projects.CoffeePeek_Gateway>(AppResources.Gateway)
    .WithReference(authService)
    .WithReference(userService)
    .WithReference(jobVacanciesService)
    .WithReference(shopsService)
    .WithReference(moderationService)
    .WithReference(outboxService)
    .WithEnvironment("DOTNET_ASPIRE", "true")
    .WithEnvironment("DOTNET_ASPIRE_RUNNING", "true");

builder.Build().Run();