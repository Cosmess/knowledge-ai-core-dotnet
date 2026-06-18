# knowledge-ai-core (.NET)

Reescrita em .NET/C# do backend RAG `knowledge-ai-core` (originalmente NestJS/TypeScript). Mantém a mesma proposta — API de chat com retrieval, servidor MCP, ingestão de Markdown/Confluence e busca vetorial em pgvector — corrigindo divergências reais encontradas entre a documentação original e a implementação:

- MCP→API agora autentica via `X-Api-Key` (era sem autenticação nenhuma).
- OpenAI, Anthropic e Ollama têm provedores de chat/embedding reais (só OpenAI funcionava de fato).
- Mascaramento de dados sensíveis plugado no pipeline de log do Serilog (existia como função solta, não usada).
- Reindexação incremental do Confluence comparando `version`/`updatedAt` (antes era sempre reingestão completa).
- Cobertura de testes real: unitários para o mediador/regras de negócio e um teste de integração contra Postgres+pgvector via Testcontainers.

A documentação de design original (SDD) foi trazida para [`docs/`](docs/README.md) e permanece válida como referência de domínio, contratos e taxonomia — as seções abaixo descrevem o que mudou na reescrita.

## Arquitetura

Clean Architecture com CQRS, sem MediatR — um mediador próprio em `KnowledgeAi.Application/Common/Mediator`:

```
src/
  KnowledgeAi.Domain          # Entidades, value objects, sem dependências externas
  KnowledgeAi.Application     # Commands/Queries (CQRS), handlers, ports (interfaces)
  KnowledgeAi.Infrastructure  # Implementações: Postgres/Dapper, LLM providers, Redis, ingestão, observabilidade
  KnowledgeAi.Api             # ASP.NET Core Web API (controllers finos, só chamam o mediador)
  KnowledgeAi.Mcp             # Servidor MCP (processo separado), chama a Api via HTTP autenticado
tests/
  KnowledgeAi.UnitTests
  KnowledgeAi.IntegrationTests
```

Cada slice de Application (ex.: `Chat/Commands/AskQuestionCommand.cs`) tem o record do command/query em um arquivo, o resultado (quando há) em outro, e o handler em outro — nada de record + handler misturados no mesmo arquivo. Em `Infrastructure/Persistence`, cada repositório vive na sua própria pasta (`Documents/`, `Users/`, `Chat/`, `Feedback/`, `IngestionJobs/`); o schema SQL fica em `Persistence/Migrations/Schema.sql`.

### Por que mediador próprio em vez de MediatR

Decisão explícita do projeto: implementar `IRequest<TResponse>`, `IRequestHandler<TRequest,TResponse>`, `IPipelineBehavior` e `Mediator` (com cache de wrappers por tipo em `ConcurrentDictionary`) sem depender da lib. Os behaviors de logging e validação (FluentValidation) ficam em `Application/Common/Behaviors`.

### Acesso a dados

Dapper + SQL raw sobre Npgsql, não EF Core — dá controle total sobre a query de similarity search em pgvector (operador `<=>`, `Score = 1 - distance`). Dois detalhes não óbvios, encontrados pelo teste de integração:

- Dapper não mapeia colunas `snake_case` para propriedades `PascalCase` por padrão; é preciso `DefaultTypeMap.MatchNamesWithUnderscores = true` (configurado em `NpgsqlDataSourceFactory`).
- Em um banco novo, a conexão que executa `create extension vector` já carrega o catálogo de tipos do Npgsql *antes* da extensão existir; sem um `connection.ReloadTypesAsync()` depois do schema, parâmetros `Pgvector.Vector` falham com "vector type not found" mesmo com a extensão criada.

## Stack

| Camada | Tecnologia |
|---|---|
| API | ASP.NET Core 8 |
| Dados | PostgreSQL + pgvector, Dapper, Npgsql, `Pgvector.Dapper` |
| Cache | Redis (StackExchange.Redis) |
| LLM | OpenAI SDK, Anthropic.SDK, OllamaSharp |
| Ingestão | Markdig (Markdown), HtmlAgilityPack (normalização HTML do Confluence), Polly (retry) |
| Auth | JWT (`System.IdentityModel.Tokens.Jwt`), PBKDF2 para senha |
| Observabilidade | Serilog, OpenTelemetry, `prometheus-net` |
| MCP | SDK oficial `ModelContextProtocol` (stdio) |
| Testes | xUnit, FluentAssertions, Testcontainers.PostgreSql (imagem `pgvector/pgvector:pg16`) |

## Status atual

| Fase | Conteúdo | Status |
|---|---|---|
| A | Domain + Application (entidades, mediador, slices CQRS, ports) | Concluída |
| B | Infrastructure (Postgres/Dapper, LLM providers, ingestão, Redis, auth, observabilidade) | Concluída — testada com Testcontainers |
| C | Api (JWT, controllers, Swagger, Serilog/OpenTelemetry/prometheus-net wiring) | Pendente |
| D | Mcp (SDK oficial, 6 tools, header `X-Api-Key`) | Pendente |
| E | Testes ponta a ponta com `WebApplicationFactory`, docker-compose | Pendente |

`KnowledgeAi.Api` e `KnowledgeAi.Mcp` hoje são apenas o scaffold padrão do template (`Program.cs` default) — controllers, autenticação e as tools MCP ainda não foram implementados.

## Rodando localmente

Pré-requisitos: .NET 8 SDK, Docker (para Postgres+pgvector via Testcontainers nos testes, ou para subir os serviços manualmente).

```bash
dotnet build
dotnet test
```

Configuração esperada (ainda não wirada em `appsettings.json`, mas já lida pelas `Options` em `Infrastructure`):

```json
{
  "Postgres": { "ConnectionString": "Host=localhost;Database=knowledgeai;Username=postgres;Password=postgres" },
  "Redis": { "ConnectionString": "localhost:6379" },
  "Jwt": { "SigningKey": "...", "Issuer": "knowledge-ai-core", "Audience": "knowledge-ai-clients" },
  "LlmProviders": { "ChatProvider": "OpenAi", "OpenAiApiKey": "..." },
  "Confluence": { "BaseUrl": "https://your-domain.atlassian.net/wiki/", "Email": "...", "ApiToken": "..." }
}
```

## Documentação

O SDD original está em [`docs/`](docs/README.md): arquitetura, contratos de API, tools MCP, desenho do RAG, modelo de dados, segurança, operação e roadmap. Ele descreve o domínio e os contratos visados pela reescrita; partes referentes a detalhes de implementação NestJS (ex.: nomes de guards, módulos Nest) não se aplicam 1:1 à árvore de código .NET acima.
