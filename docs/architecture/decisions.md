# Decisões Técnicas

## Divisão De Repositórios

Serão usados dois repositórios:

```txt
knowledge-ai-core
knowledge-ai-web
```

`knowledge-ai-core` contém backend, MCP, RAG e infraestrutura.

`knowledge-ai-web` contém o frontend Next.js.

## Estratégia De Contratos

A API expõe OpenAPI/Swagger. O frontend deve gerar client e tipos a partir do contrato OpenAPI, evitando cópia manual de DTOs.

## Vector Store

O MVP usa PostgreSQL + pgvector.

Outros stores, como Qdrant, Pinecone ou Weaviate, podem ser adicionados futuramente atrás da abstração `vector-store`.

## Estratégia MCP

O MCP Server chama a Knowledge API. Isso mantém retrieval, autorização e filtros de fontes centralizados na API.

## Estratégia De Autenticação

A API é responsável pelas decisões de autorização. O frontend pode esconder ações na interface, mas não é fonte confiável de permissão.
