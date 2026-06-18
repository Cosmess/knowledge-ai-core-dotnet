using KnowledgeAi.Mcp.Configuration;
using KnowledgeAi.Mcp.Search;
using KnowledgeAi.Mcp.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddConsole(options => options.LogToStandardErrorThreshold = LogLevel.Trace);

var apiOptions = new KnowledgeApiOptions
{
    BaseUrl = Environment.GetEnvironmentVariable("KNOWLEDGE_API_BASE_URL") ?? "http://localhost:5080",
    ApiKey = Environment.GetEnvironmentVariable("KNOWLEDGE_API_KEY")
        ?? throw new InvalidOperationException("KNOWLEDGE_API_KEY environment variable is required."),
};

builder.Services.AddHttpClient<KnowledgeApiClient>(client =>
{
    client.BaseAddress = new Uri(apiOptions.BaseUrl.TrimEnd('/') + '/');
    client.DefaultRequestHeaders.Add("X-Api-Key", apiOptions.ApiKey);
});

builder.Services.AddSingleton<KnowledgeSearchTools>();

builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithTools<KnowledgeSearchTools>();

await builder.Build().RunAsync();
