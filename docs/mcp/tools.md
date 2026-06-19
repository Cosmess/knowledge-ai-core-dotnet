# Tools MCP

6 tools implementadas (`[McpServerToolType]`, classe `KnowledgeSearchTools` em `src/KnowledgeAi.Mcp/Tools`):

```txt
search_technical_docs      -> domain: technical
search_business_rules      -> domain: business
search_api_docs            -> domain: api
search_architecture_docs   -> domain: architecture
search_user_stories        -> domain: backlog
get_service_context        -> domain: technical
```

Cada tool fixa o `domain` indicado e `audience: developers`; os demais parâmetros vêm do chamador.

## Input Schema

```json
{
  "query": "string",
  "system": "string",
  "spaceKey": "string",
  "limit": 5
}
```

Apenas `query` é obrigatório; os demais são opcionais (`limit` default 5).

## Output

A tool retorna o JSON serializado de `McpSearchResult` como conteúdo de texto:

```json
{
  "query": "string",
  "domain": "technical",
  "results": [
    { "title": "string", "url": "string", "score": 0.0 }
  ],
  "evidenceStatus": "insufficient"
}
```

`evidenceStatus` é `"found"` ou `"insufficient"` (threshold de similaridade 0.15 sobre o melhor resultado).

## Autorização

A chamada HTTP feita pela tool para `POST /mcp/search` carrega `X-Api-Key` (identidade do processo MCP) e `Authorization: Bearer` (JWT do usuário final, via `KNOWLEDGE_USER_JWT`). A API restringe a busca aos `AllowedSpaceKeys` do usuário do token; se `spaceKey` for pedido e não estiver nessa lista, a chamada falha com `403` (a tool propaga o erro HTTP).
