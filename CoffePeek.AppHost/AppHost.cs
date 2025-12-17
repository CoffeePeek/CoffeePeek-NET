using CoffeePeek.Shared.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder
    .AddPostgres(AppResources.Postgres)
    .WithDataVolume();

var authDb = postgres.AddDatabase(AppResources.AuthDb);
var usersDb = postgres.AddDatabase(AppResources.UserDb);
var jobsDb = postgres.AddDatabase(AppResources.JobVacanciesDb);
var shopsDb = postgres.AddDatabase(AppResources.ShopsDb);
var moderationDb = postgres.AddDatabase(AppResources.ModerationDb);

var redis = builder
    .AddRedis(AppResources.RedisCache)
    .WithDataVolume();

var rabbit = builder
    .AddRabbitMQ(AppResources.RabbitMq)
    .WithManagementPlugin();

builder.AddProject<Projects.CoffeePeek_AuthService>(AppResources.AuthService)
    .WithReference(authDb)
    .WithReference(redis)
    .WithReference(rabbit);

builder.AddProject<Projects.CoffeePeek_UserService>(AppResources.UserService)
    .WithReference(usersDb)
    .WithReference(redis)
    .WithReference(rabbit);

builder.AddProject<Projects.CoffeePeek_JobVacancies>(AppResources.JobVacanciesService)
    .WithReference(jobsDb)
    .WithReference(redis)
    .WithReference(rabbit);

builder.AddProject<Projects.CoffeePeek_ShopsService>(AppResources.ShopsService)
    .WithReference(shopsDb)
    .WithReference(redis)
    .WithReference(rabbit);

builder.AddProject<Projects.CoffeePeek_ModerationService>(AppResources.ModerationService)
    .WithReference(moderationDb)
    .WithReference(redis)
    .WithReference(rabbit);

builder.AddProject<Projects.OutboxBackgroundService>(AppResources.OutboxBackgroundService)
    .WithReference(rabbit)
    .WithReference(redis);

builder.AddProject<Projects.CoffeePeek_Gateway>(AppResources.Gateway)
    .WithReference(redis);

builder.Build().Run();