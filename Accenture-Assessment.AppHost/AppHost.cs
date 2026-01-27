var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedis("cache");

var postgres = builder.AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var database = postgres.AddDatabase("holidaysdb");

var apiService = builder.AddProject<Projects.Accenture_Assessment_ApiService>("apiservice")
    .WithHttpHealthCheck("/health")
    .WithReference(database)
    .WaitFor(database);

builder.AddProject<Projects.Accenture_Assessment_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(cache)
    .WaitFor(cache)
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
