# Tools MCP

Tools iniciais:

```txt
search_technical_docs
search_business_rules
search_api_docs
search_architecture_docs
search_user_stories
get_service_context
```

## Input Schema

```json
{
  "query": "string",
  "system": "string",
  "spaceKey": "string",
  "limit": 5
}
```

Apenas `query` é obrigatório.

## Output

O MCP Server retorna JSON estruturado como conteúdo de texto:

```json
{
  "query": "string",
  "domain": "technical",
  "results": [],
  "evidenceStatus": "insufficient"
}
```

## Regra

As tools MCP devem retornar fontes e evidências. Se não houver evidência suficiente, devem informar isso explicitamente.
