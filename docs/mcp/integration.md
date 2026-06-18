# Integração MCP

O MCP Server usa transporte `stdio` e chama a Knowledge API.

```txt
IDE/Agent
  -> MCP tool
  -> dev-knowledge-mcp
  -> Knowledge API /mcp/search
  -> structured context
```

## Ambiente

```env
KNOWLEDGE_API_BASE_URL=http://localhost:3000
```

## Por Que O MCP Chama A API

Centralizar o retrieval na API mantém:

- autorização
- filtros de documentos
- ranking de fontes
- logs
- LLMOps
- mascaramento de dados sensíveis

em um único caminho de backend.
