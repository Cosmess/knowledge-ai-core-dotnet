# Componentes

A solução `KnowledgeAi.sln` segue Clean Architecture, organizada em projetos `src/`. Não existem mais pastas `apps/` ou `packages/` (estrutura do projeto NestJS anterior) — cada responsabilidade vive como classes dentro dos projetos abaixo.

## src/KnowledgeAi.Domain

Núcleo do domínio: entidades (`Document`, `DocumentChunk`, `ChatSession`, `ChatMessage`, `Feedback`, `User`, etc.) e value objects/enums (`DocumentSource`, `DocumentType`, `KnowledgeDomain`, `Audience`). Sem dependências externas.

## src/KnowledgeAi.Application

Orquestra os casos de uso via commands/queries e handlers (um mediator próprio, no estilo CQRS), com validação de entrada via FluentValidation. Principais handlers:

- Chat: `AskQuestionCommandHandler` — classificação implícita por domínio, retrieval, montagem de prompt, chamada ao LLM e validação de evidência.
- Busca MCP: `SearchMcpQueryHandler` — usado pelo endpoint `/mcp/search`.
- Ingestão: handlers para ingestão de Markdown e de Confluence.
- Auth, Feedback e Documents: casos de uso de autenticação, feedback de respostas e consulta/gestão de documentos.

## src/KnowledgeAi.Infrastructure

Implementações concretas usadas pela Application:

- Persistência: `DocumentRepository`, `UserRepository` e repositórios análogos, usando Dapper + Npgsql sobre PostgreSQL com pgvector.
- Cache: Redis via StackExchange.Redis.
- Provedores de LLM/embedding: OpenAI, Anthropic e Ollama (não há integração com Azure OpenAI).
- Observabilidade: Serilog (com mascaramento de dados sensíveis já ativo), OpenTelemetry (tracing e métricas, hoje com exporter de console, sem backend OTLP configurado) e Prometheus (`prometheus-net`, endpoint `/metrics`).
- Ingestão: carregamento de Markdown (`IMarkdownLoader`, via Markdig), cliente Confluence (`IConfluenceClient`), normalização de HTML (`HtmlAgilityPackNormalizer`), sanitização de conteúdo e chunking (`HeadingChunkingService`).

## src/KnowledgeAi.Api

API ASP.NET Core. Controllers: `Auth`, `Chat`, `Documents`, `Feedback`, `Ingestion`, `Mcp`, `Health`. Autenticação dupla: JWT Bearer (usuários) e API Key (serviços/MCP, combinada com JWT de usuário — ver `decisions.md`). Swagger disponível em `/swagger`, somente em ambiente de Development.

## src/KnowledgeAi.Mcp

Servidor MCP via stdio, usando o SDK oficial `ModelContextProtocol.Server`. Expõe 6 tools (`search_technical_docs`, `search_business_rules`, `search_api_docs`, `search_architecture_docs`, `search_user_stories`, `get_service_context`). O MCP Server não duplica a lógica de RAG: cada tool delega para a Knowledge API via HTTP (`KnowledgeApiClient` → `POST /mcp/search`), enviando tanto a API Key (`KNOWLEDGE_API_KEY`) quanto um JWT de usuário (`KNOWLEDGE_USER_JWT`), para que a API consiga aplicar autorização por `spaceKey` por usuário real, em vez de tratar todas as chamadas MCP como uma única identidade de serviço compartilhada.
