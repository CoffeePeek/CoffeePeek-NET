using CoffeePeek.Shared.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var accountService = builder.AddProject<Projects.CoffeePeek_AccountService>(AppResources.AccountService);


var jobVacanciesService = builder.AddProject<Projects.CoffeePeek_JobVacancies>(AppResources.JobVacanciesService);

var shopsService = builder.AddProject<Projects.CoffeePeek_ShopsService>(AppResources.ShopsService);

var moderationService = builder.AddProject<Projects.CoffeePeek_ModerationService>(AppResources.ModerationService);

var outboxService = builder.AddProject<Projects.OutboxBackgroundService>(AppResources.OutboxBackgroundService);

builder.AddProject<Projects.CoffeePeek_Gateway>(AppResources.Gateway)
    .WithReference(accountService)
    .WithReference(jobVacanciesService)
    .WithReference(shopsService)
    .WithReference(moderationService)
    .WithReference(outboxService)
    .WithEnvironment("DOTNET_ASPIRE", "true")
    .WithEnvironment("DOTNET_ASPIRE_RUNNING", "true");

builder.Build().Run();