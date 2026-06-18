using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Infrastructure.Auth;
using KnowledgeAi.Infrastructure.Caching;
using KnowledgeAi.Infrastructure.Ingestion;
using KnowledgeAi.Infrastructure.LlmProviders;
using KnowledgeAi.Infrastructure.Observability;
using KnowledgeAi.Infrastructure.Persistence;
using KnowledgeAi.Infrastructure.Persistence.Chat;
using KnowledgeAi.Infrastructure.Persistence.Documents;
using KnowledgeAi.Infrastructure.Persistence.Feedback;
using KnowledgeAi.Infrastructure.Persistence.IngestionJobs;
using KnowledgeAi.Infrastructure.Persistence.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Npgsql;
using Polly;
using Polly.Extensions.Http;
using StackExchange.Redis;

namespace KnowledgeAi.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddPersistence(services, configuration);
        AddLlmProviders(services, configuration);
        AddIngestion(services, configuration);
        AddCaching(services, configuration);
        AddAuthInfrastructure(services, configuration);

        services.AddKnowledgeAiObservability();

        return services;
    }

    private static void AddPersistence(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<NpgsqlOptions>(configuration.GetSection(NpgsqlOptions.SectionName));

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<NpgsqlOptions>>().Value;
            return NpgsqlDataSourceFactory.Create(options.ConnectionString);
        });

        services.AddSingleton<DatabaseInitializer>();
        services.AddScoped<IDocumentRepository, DocumentRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IChatRepository, ChatRepository>();
        services.AddScoped<IFeedbackRepository, FeedbackRepository>();
        services.AddScoped<IIngestionJobRepository, IngestionJobRepository>();
    }

    private static void AddLlmProviders(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<LlmProviderOptions>(configuration.GetSection(LlmProviderOptions.SectionName));

        services.AddSingleton<OpenAiLlmProvider>();
        services.AddSingleton<AnthropicLlmProvider>();
        services.AddSingleton<OllamaLlmProvider>();
        services.AddSingleton<OpenAiEmbeddingProvider>();
        services.AddSingleton<OllamaEmbeddingProvider>();

        services.AddSingleton<ILlmProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<LlmProviderOptions>>().Value;
            return options.ChatProvider switch
            {
                ChatProviderKind.Anthropic => sp.GetRequiredService<AnthropicLlmProvider>(),
                ChatProviderKind.Ollama => sp.GetRequiredService<OllamaLlmProvider>(),
                _ => sp.GetRequiredService<OpenAiLlmProvider>(),
            };
        });

        services.AddSingleton<IEmbeddingProvider>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<LlmProviderOptions>>().Value;
            return options.EmbeddingProvider switch
            {
                EmbeddingProviderKind.Ollama => sp.GetRequiredService<OllamaEmbeddingProvider>(),
                _ => sp.GetRequiredService<OpenAiEmbeddingProvider>(),
            };
        });
    }

    private static void AddIngestion(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IMarkdownLoader, MarkdigMarkdownLoader>();
        services.AddSingleton<IHtmlNormalizer, HtmlAgilityPackNormalizer>();
        services.AddSingleton<IChunkingService, HeadingChunkingService>();

        services.Configure<ConfluenceOptions>(configuration.GetSection(ConfluenceOptions.SectionName));

        services.AddHttpClient<IConfluenceClient, ConfluenceHttpClient>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<ConfluenceOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);

                if (!string.IsNullOrWhiteSpace(options.Email) && !string.IsNullOrWhiteSpace(options.ApiToken))
                {
                    var credentials = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes($"{options.Email}:{options.ApiToken}"));
                    client.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");
                }
            })
            .AddPolicyHandler(HttpPolicyExtensions
                .HandleTransientHttpError()
                .WaitAndRetryAsync(3, attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt))));
    }

    private static void AddCaching(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection(RedisOptions.SectionName));

        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var options = sp.GetRequiredService<IOptions<RedisOptions>>().Value;
            return ConnectionMultiplexer.Connect(options.ConnectionString);
        });

        services.AddSingleton<ICacheService, RedisCacheService>();
    }

    private static void AddAuthInfrastructure(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
    }
}
