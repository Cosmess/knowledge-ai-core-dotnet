using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Domain.Entities;
using KnowledgeAi.Domain.ValueObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace KnowledgeAi.IntegrationTests.Api;

public sealed class KnowledgeApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string ApiKeyValue = "integration-test-api-key";

    private readonly PostgreSqlContainer _container = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg16")
        .WithDatabase("knowledgeai")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    public async Task InitializeAsync() => await _container.StartAsync();

    public new async Task DisposeAsync()
    {
        await _container.DisposeAsync();
        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Postgres:ConnectionString"] = _container.GetConnectionString(),
                ["ApiKey:Value"] = ApiKeyValue,
            });
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IEmbeddingProvider>();
            services.AddSingleton<IEmbeddingProvider, FakeEmbeddingProvider>();

            services.RemoveAll<ILlmProvider>();
            services.AddSingleton<ILlmProvider, FakeLlmProvider>();
        });
    }

    public string CreateAccessTokenFor(Role role, params string[] allowedSpaceKeys)
    {
        var generator = Services.GetRequiredService<IJwtTokenGenerator>();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"{Guid.NewGuid()}@example.com",
            PasswordHash = "unused",
            Role = role,
            AllowedSpaceKeys = allowedSpaceKeys.ToHashSet(),
        };

        return generator.GenerateToken(user).Value;
    }
}
