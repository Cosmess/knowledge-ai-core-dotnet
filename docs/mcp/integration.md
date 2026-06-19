# Integração MCP

O servidor MCP (`KnowledgeAi.Mcp`) usa transporte `stdio` (SDK oficial `ModelContextProtocol.Server`) e chama a Knowledge API via HTTP — não duplica lógica de RAG.

```txt
IDE/Agent
  -> MCP tool (ex: search_technical_docs)
  -> KnowledgeAi.Mcp (stdio)
  -> Knowledge API POST /mcp/search   (headers: X-Api-Key + Authorization: Bearer)
  -> resultados estruturados (title, url, score, evidenceStatus)
```

## Ambiente

```env
KNOWLEDGE_API_BASE_URL=http://localhost:5080
KNOWLEDGE_API_KEY=<chave fixa do processo MCP>
KNOWLEDGE_USER_JWT=<JWT do usuário final>
```

## Autenticação MCP → API

`POST /mcp/search` exige os dois mecanismos simultaneamente:

- `X-Api-Key`: prova que a chamada vem do processo MCP confiável.
- `Authorization: Bearer <KNOWLEDGE_USER_JWT>`: identifica o usuário final por trás do processo MCP, permitindo que a API aplique `AllowedSpaceKeys` por usuário (sem essa identidade, não haveria como restringir a busca aos spaces que o usuário de fato pode acessar).

Faltando qualquer um dos dois, a API responde `401 Unauthorized`. Se o `spaceKey` pedido não estiver entre os spaces permitidos do usuário do token, responde `403 Forbidden`.

## Por Que O MCP Chama A API

Centralizar o retrieval na API mantém em um único caminho de backend:

- autorização (role + spaceKey)
- filtros de documentos (domain, audience, system)
- threshold de evidência (0.15)
- mascaramento de dados sensíveis nos logs
- observabilidade (Serilog, OpenTelemetry, Prometheus)
