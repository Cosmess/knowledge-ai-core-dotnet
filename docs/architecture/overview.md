# Visão de Arquitetura

`knowledge-ai-core` é o repositório backend da Knowledge AI Platform.

Ele contém:

- API NestJS.
- MCP Server para IDEs e agentes de IA.
- Orquestração RAG.
- Ingestão de Markdown e Confluence.
- Busca vetorial com PostgreSQL + pgvector.
- Cache Redis.
- Abstração de provedores de LLM.
- Segurança, observabilidade e LLMOps.

O frontend fica separado no repositório `knowledge-ai-web` e consome esta API por chamadas HTTP autenticadas.

## Fluxo De Alto Nível

```txt
Confluence / Markdown / OpenAPI
        |
        v
Ingestion
        |
        v
Chunking + Metadata
        |
        v
Embeddings
        |
        v
PostgreSQL + pgvector
        |
        v
Retrieval
        |
        v
RAG Orchestrator
        |
        +--> Knowledge API
        |
        +--> MCP Server
```
