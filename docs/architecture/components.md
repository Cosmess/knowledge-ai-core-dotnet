# Componentes

## apps/knowledge-api

API NestJS responsável por:

- `/health`
- `/chat`
- `/mcp/search`
- endpoints de ingestão
- endpoints de documentos
- endpoints de feedback
- autenticação e autorização
- geração de OpenAPI

## apps/dev-knowledge-mcp

Servidor MCP responsável por expor tools para agentes e IDEs.

O MCP Server não deve duplicar a lógica de RAG. Ele deve chamar a Knowledge API e retornar contexto estruturado com fontes.

## packages/rag-core

Orquestra:

- classificação de perguntas
- retrieval
- montagem de prompt
- chamadas ao LLM
- validação da resposta
- comportamento de fallback

## packages/shared-types

Contratos compartilhados pela API, MCP e packages internos.

## packages/vector-store

Abstração de vector store. O alvo do MVP é PostgreSQL + pgvector.

## packages/llm-providers

Abstração de provedores para OpenAI, Claude, Azure OpenAI e Ollama.

## packages/confluence-client

Cliente para Confluence REST API.

## packages/markdown-loader

Leitor, parser, extrator de frontmatter e normalizador de Markdown.

## packages/prompt-templates

Templates de prompts versionados por audiência e domínio.

## packages/observability

Helpers para logs, mascaramento, métricas e tracing.
