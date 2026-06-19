# Plano De Implementação

O projeto original (NestJS, monorepo `apps/`+`packages/`) foi reescrito em .NET, seguindo Clean Architecture. As fases abaixo refletem a reescrita real, identificadas pelos commits do repositório `knowledge-ai-core-dotnet`. As fases antigas "1 a 6" (Scaffold, Backend API, Markdown RAG, MCP, Confluence, Segurança/LLMOps) pertenciam ao projeto NestJS anterior e não se aplicam mais.

## Fase A: Domínio E Scaffold

- Criar a solução `KnowledgeAi.sln`.
- Criar `src/KnowledgeAi.Domain` com entidades e value objects/enums (`DocumentSource`, `DocumentType`, `KnowledgeDomain`, `Audience`).
- Criar `src/KnowledgeAi.Application` com a estrutura de commands/queries/handlers (mediator próprio) e FluentValidation.
- Trazer a documentação SDD original e adaptar o README para a reescrita .NET.

Status: concluída.

## Fase B: Infrastructure

- Persistência em PostgreSQL + pgvector via Dapper/Npgsql (`DocumentRepository`, `UserRepository`, etc.).
- Schema aplicado por script SQL idempotente (`Schema.sql`) via `DatabaseInitializer`, com índice HNSW para os embeddings.
- Cache Redis via StackExchange.Redis.
- Provedores de LLM/embedding: OpenAI, Anthropic e Ollama.
- Pipeline de ingestão de Markdown (`IMarkdownLoader`/Markdig) e Confluence (`IConfluenceClient`), com chunking por heading e por orçamento de palavras (`HeadingChunkingService`).

Status: concluída.

## Fase C: Api

- Controllers: Auth, Chat, Documents, Feedback, Ingestion, Mcp, Health.
- Autenticação dupla: JWT Bearer e API Key.
- Swagger via Swashbuckle, disponível em `/swagger` (Development).
- Observabilidade: Serilog, OpenTelemetry e Prometheus (`/metrics`).

Status: concluída (commit `4729dc0`).

## Fase D: Mcp Server

- Servidor MCP via SDK oficial `ModelContextProtocol`, stdio.
- 6 tools: `search_technical_docs`, `search_business_rules`, `search_api_docs`, `search_architecture_docs`, `search_user_stories`, `get_service_context`.
- Cada tool delega para a Knowledge Api via HTTP (`KnowledgeApiClient` → `POST /mcp/search`), sem duplicar lógica de RAG.
- Testado de ponta a ponta via stdio JSON-RPC (`initialize`, `tools/list`, `tools/call`).

Status: concluída (commit `c899592`).

## Fase E: Testes

- Testes de validators, chunking, evidence-threshold e testes end-to-end.

Status: concluída (commit `a804ded`).

## Fase F: Containerização

- Dockerfiles para Api e Mcp.
- Integração de ambos ao `docker-compose`.

Status: concluída (commit `3f48fa4`).

## Correções Recentes De Segurança E Qualidade De Ingestão

Rodada de correções aplicada após a Fase F, ainda sobre a base das fases A-F:

- Autorização por `spaceKey` passou a ser de fato aplicada em `/chat` e em `/mcp/search`, respeitando `AllowedSpaceKeys` do usuário.
- O MCP Server passou a propagar a identidade do usuário (JWT, via `KNOWLEDGE_USER_JWT`) além da API Key de serviço (`KNOWLEDGE_API_KEY`), permitindo que a Api aplique a mesma autorização por `spaceKey` também a chamadas originadas do MCP.
- Conteúdo ingerido (Markdown e HTML do Confluence) passou a ser sanitizado antes do chunking, removendo tags (`<script>`, `<style>`, `<iframe>`, `<object>`, `<embed>`, `<noscript>`) e atributos perigosos (`on*=`, `javascript:`).
- A normalização de HTML do Confluence passou a converter `<h1>`-`<h6>` em prefixos Markdown antes da extração de texto, preservando o `headingPath` nos chunks de páginas Confluence (antes, esse metadado era perdido para essa fonte).

Status: concluída.

## Pendências Conhecidas

- Backend OTLP para OpenTelemetry ainda não está configurado (hoje o exporter é apenas console).
- Avaliação automatizada/avançada de qualidade de respostas (LLMOps) ainda não foi implementada.
- Critérios que dependem do frontend (`knowledge-ai-web`) estão fora do escopo deste repositório.
