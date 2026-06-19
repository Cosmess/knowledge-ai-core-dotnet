# knowledge-ai-core (.NET)

Reescrita em .NET/C# do backend RAG `knowledge-ai-core` (originalmente NestJS/TypeScript). Mantém a mesma proposta — API de chat com retrieval, servidor MCP, ingestão de Markdown/Confluence e busca vetorial em pgvector — corrigindo divergências reais encontradas entre a documentação original e a implementação:

- MCP→API agora autentica via `X-Api-Key` (era sem autenticação nenhuma).
- OpenAI, Anthropic e Ollama têm provedores de chat/embedding reais (só OpenAI funcionava de fato).
- Mascaramento de dados sensíveis plugado no pipeline de log do Serilog (existia como função solta, não usada).
- Reindexação incremental do Confluence comparando `version`/`updatedAt` (antes era sempre reingestão completa).
- Cobertura de testes real: unitários para mediador, validators, chunking e logica de threshold de evidencia, mais testes de integracao ponta a ponta (/chat, /mcp/search, /ingest/markdown) com WebApplicationFactory + Postgres/pgvector via Testcontainers.

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
| Testes | xUnit, FluentAssertions, NSubstitute, Testcontainers.PostgreSql (imagem `pgvector/pgvector:pg16`), `Microsoft.AspNetCore.Mvc.Testing` |

## Status atual

| Fase | Conteúdo | Status |
|---|---|---|
| A | Domain + Application (entidades, mediador, slices CQRS, ports) | Concluída |
| B | Infrastructure (Postgres/Dapper, LLM providers, ingestão, Redis, auth, observabilidade) | Concluída — testada com Testcontainers |
| C | Api (JWT, controllers, Swagger, Serilog/OpenTelemetry/prometheus-net wiring) | Concluída |
| D | Mcp (SDK oficial, 6 tools, header `X-Api-Key`) | Concluída |
| E | Testes (unit + integracao com `WebApplicationFactory`), docker-compose | Concluida |
| F | Autorização por space, identidade no MCP, seed de admin, sanitização, hybrid search/reranking, LLMOps, Grafana/Prometheus | Concluída |

`KnowledgeAi.Api` expõe os 12 endpoints documentados com JWT Bearer + scheme `ApiKey` dedicado para `/mcp/search` (esse endpoint agora exige os dois ao mesmo tempo — ver abaixo). `KnowledgeAi.Mcp` usa o SDK oficial `ModelContextProtocol` via stdio e chama a Api autenticado com `X-Api-Key` **e** um JWT de usuário final (`KNOWLEDGE_USER_JWT`), para que a Api consiga aplicar autorização por `spaceKey` por usuário também nesse caminho. `dotnet test` na solution roda 46 testes unitários e 15 de integração (todos via Testcontainers, sem dependência de serviços externos rodando localmente); `docker-compose.yml` sobe Postgres+pgvector, Redis, Ollama, Api, Mcp, Prometheus e Grafana como containers — todos já wireados (Prometheus faz scrape de `api:8080/metrics`; Grafana provisiona datasource e dashboard automaticamente a partir de `ops/grafana/provisioning/`).

### Correções de segurança e qualidade

- `/chat` e `/mcp/search` agora aplicam `AllowedSpaceKeys` do usuário autenticado: pedir um `spaceKey` fora dessa lista retorna `403 Forbidden` (antes, qualquer usuário autenticado podia consultar qualquer space).
- `/mcp/search` agora exige um JWT de usuário (`KNOWLEDGE_USER_JWT`) além da `X-Api-Key` compartilhada, para ter identidade real por chamada.
- Chunking de páginas Confluence agora preserva `headingPath` (a normalização HTML converte `<h1>`-`<h6>` em headings estilo Markdown antes do chunking).
- Conteúdo ingerido (Markdown e Confluence) passa por sanitização (`HtmlContentSanitizer`) antes do chunking, removendo tags/atributos perigosos (`<script>`, `<style>`, `on*=`, `javascript:`).

### Roadmap implementado nesta rodada

- **Seed de admin**: a Api cria um usuário `Admin` no startup a partir de `AdminSeed:Email`/`AdminSeed:Password` (idempotente; pulado se não configurado) — sem isso, um banco novo não tinha como logar.
- **Grafana + Prometheus**: serviços reais no `docker-compose.yml`, configuração em `ops/` (ver `docs/operations/docker.md`).
- **Métricas de LLMOps**: tokens de entrada/saída, custo estimado (configurável via `LlmPricing`), taxa de fallback e taxa de evidência insuficiente, expostas em `/metrics` (ver `docs/operations/llmops.md`).
- **Busca híbrida + reranking**: `DocumentRepository.SearchAsync` agora combina similaridade vetorial (pgvector/HNSW) com full-text search (`tsvector`/GIN, `ts_rank_cd`) em duas etapas — kNN amplo via índice, depois rerank do conjunto candidato (ver `docs/rag/retrieval.md`).

## Rodando localmente

Pré-requisitos: .NET 8 SDK, Docker (para Postgres+pgvector via Testcontainers nos testes, ou para subir os serviços manualmente).

```bash
dotnet build
dotnet test
```

Configuração em `src/KnowledgeAi.Api/appsettings.json` (valores de placeholder — preencha `OpenAiApiKey`/`AnthropicApiKey`/`ApiKey:Value`/`Jwt:SigningKey`/`AdminSeed:Password` com segredos reais antes de usar fora de dev local):

```json
{
  "Postgres": { "ConnectionString": "Host=localhost;Database=knowledgeai;Username=postgres;Password=postgres" },
  "Redis": { "ConnectionString": "localhost:6379" },
  "Jwt": { "SigningKey": "...", "Issuer": "knowledge-ai-core", "Audience": "knowledge-ai-clients" },
  "ApiKey": { "Value": "..." },
  "AdminSeed": { "Email": "admin@example.com", "Password": "...", "SpaceKeys": ["ENG"] },
  "LlmProviders": { "ChatProvider": "OpenAi", "OpenAiApiKey": "..." },
  "LlmPricing": { "Providers": { "openai": { "InputPricePerThousandTokens": 0, "OutputPricePerThousandTokens": 0 } } },
  "Confluence": { "BaseUrl": "https://your-domain.atlassian.net/wiki/", "Email": "...", "ApiToken": "..." }
}
```

`AdminSeed` cria um usuário `Admin` no startup se nenhum usuário com aquele e-mail existir (idempotente); deixe `Email`/`Password` vazios para pular. `LlmPricing` é opcional — sem ele, o custo estimado nas métricas fica em `0` em vez de usar um preço chumbado e potencialmente desatualizado.

Subindo a Api e o Mcp localmente:

```bash
dotnet run --project src/KnowledgeAi.Api --urls http://localhost:5080

KNOWLEDGE_API_BASE_URL=http://localhost:5080 KNOWLEDGE_API_KEY=<mesmo valor de ApiKey:Value> \
  dotnet run --project src/KnowledgeAi.Mcp
```

O `KnowledgeAi.Mcp` conecta via `stdio` e expõe `search_technical_docs`, `search_business_rules`, `search_api_docs`, `search_architecture_docs`, `search_user_stories` e `get_service_context`, cada uma chamando `POST /mcp/search` na Api com o header `X-Api-Key`.

## Documentação

O SDD original está em [`docs/`](docs/README.md): arquitetura, contratos de API, tools MCP, desenho do RAG, modelo de dados, segurança, operação e roadmap. Ele descreve o domínio e os contratos visados pela reescrita; partes referentes a detalhes de implementação NestJS (ex.: nomes de guards, módulos Nest) não se aplicam 1:1 à árvore de código .NET acima.
