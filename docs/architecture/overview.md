# Visão de Arquitetura

`knowledge-ai-core-dotnet` é o repositório backend da Knowledge AI Platform, reescrito em .NET (solução `KnowledgeAi.sln`) seguindo Clean Architecture.

Ele contém:

- API ASP.NET Core (`src/KnowledgeAi.Api`).
- MCP Server para IDEs e agentes de IA (`src/KnowledgeAi.Mcp`), via SDK oficial `ModelContextProtocol`, stdio.
- Orquestração RAG (`src/KnowledgeAi.Application`).
- Ingestão de Markdown e Confluence (`src/KnowledgeAi.Infrastructure`).
- Busca vetorial com PostgreSQL + pgvector.
- Cache Redis.
- Provedores de LLM: OpenAI, Anthropic e Ollama.
- Segurança, observabilidade (Serilog + OpenTelemetry + Prometheus) e LLMOps.

O frontend fica separado no repositório `knowledge-ai-web` e consome esta API por chamadas HTTP autenticadas. Critérios e funcionalidades que dependem do frontend estão fora do escopo deste repositório.

## Camadas (Clean Architecture)

```txt
KnowledgeAi.Domain
        |
        v
KnowledgeAi.Application  (commands/queries, handlers, FluentValidation)
        |
        v
KnowledgeAi.Infrastructure (Postgres/pgvector, Redis, provedores LLM, observability, ingestion)
        |
        +--> KnowledgeAi.Api  (controllers, auth JWT/API Key, Swagger)
        |
        +--> KnowledgeAi.Mcp  (stdio, 6 tools, chama a Api via HTTP)
```

## Fluxo De Alto Nível

```txt
Confluence / Markdown
        |
        v
Ingestão (sanitização + normalização + chunking por heading/palavras)
        |
        v
Embeddings
        |
        v
PostgreSQL + pgvector (índice HNSW)
        |
        v
Retrieval
        |
        v
Orquestração RAG (Application)
        |
        +--> Knowledge Api (Controllers)
        |
        +--> Mcp Server (chama a Api via HTTP)
```
