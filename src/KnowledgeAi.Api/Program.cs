using System.Text.Json;
using System.Text.Json.Serialization;
using KnowledgeAi.Api.Auth;
using KnowledgeAi.Api.ExceptionHandling;
using KnowledgeAi.Api.Swagger;
using KnowledgeAi.Application;
using KnowledgeAi.Infrastructure;
using KnowledgeAi.Infrastructure.Auth;
using KnowledgeAi.Infrastructure.Observability;
using KnowledgeAi.Infrastructure.Persistence;
using Prometheus;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((_, _, loggerConfiguration) => SerilogConfiguration.Configure(loggerConfiguration));

builder.Services.AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(SwaggerSetup.Configure);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();

    var adminSeeder = scope.ServiceProvider.GetRequiredService<AdminUserSeeder>();
    await adminSeeder.SeedAsync();
}

app.UseApiExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseHttpMetrics();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapMetrics();

app.Run();

public partial class Program;
