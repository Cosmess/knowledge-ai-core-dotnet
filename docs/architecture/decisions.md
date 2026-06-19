# Decisões Técnicas

## Divisão De Repositórios

Serão usados dois repositórios:

```txt
knowledge-ai-core-dotnet
knowledge-ai-web
```

`knowledge-ai-core-dotnet` contém backend (.NET), MCP, RAG e infraestrutura.

`knowledge-ai-web` contém o frontend (fora do escopo deste repositório).

## Estratégia De Contratos

A API expõe OpenAPI/Swagger real, gerado via Swashbuckle e disponível em `/swagger` (somente em Development). O frontend deve gerar client e tipos a partir do contrato OpenAPI, evitando cópia manual de DTOs.

## Vector Store

O vector store é PostgreSQL + pgvector, acessado diretamente via Npgsql/Dapper. Não existe uma camada de abstração "vector-store" para múltiplos backends — não há suporte plugável a Qdrant, Pinecone ou Weaviate. Trocar de vector store hoje significaria substituir o acesso a dados na Infrastructure, não configurar um provider alternativo.

## Estratégia MCP

O MCP Server chama a Knowledge API via HTTP em vez de duplicar lógica de RAG. Isso mantém retrieval, autorização e filtros de fontes centralizados na API. Desde a correção mais recente, o MCP também propaga a identidade do usuário (JWT) além da API Key de serviço, para que a API possa autorizar por `spaceKey` por usuário real, e não apenas por uma identidade de serviço compartilhada.

## Estratégia De Autenticação

A API é responsável pelas decisões de autorização. Dois esquemas de autenticação coexistem:

- **JWT Bearer**: usado por usuários finais (ex.: chamadas a `/chat`), com autorização por `spaceKey` baseada na lista `AllowedSpaceKeys` do usuário.
- **API Key + JWT de usuário**: usado pelo MCP Server. A API Key identifica o processo MCP como cliente de serviço autorizado; o JWT de usuário, propagado pelo MCP, identifica a pessoa por trás da chamada para que a mesma autorização por `spaceKey` válida para `/chat` também seja aplicada em `/mcp/search`.

O frontend pode esconder ações na interface, mas não é fonte confiável de permissão.
