# Endpoints Da API

## Implementados (12)

```http
GET  /health
POST /auth/login
GET  /auth/me
POST /chat
POST /mcp/search
POST /ingest/markdown
POST /ingest/confluence
POST /reindex
GET  /documents
GET  /documents/{id}
GET  /spaces
POST /feedback
GET  /metrics
```

Autenticação/autorização por endpoint:

| Endpoint | Auth | Restrição |
|---|---|---|
| `GET /health` | nenhuma | — |
| `POST /auth/login` | nenhuma | — |
| `GET /auth/me` | JWT Bearer | qualquer role |
| `POST /chat` | JWT Bearer | `spaceKey` precisa estar em `allowedSpaceKeys` do usuário (senão 403) |
| `POST /mcp/search` | `X-Api-Key` **e** JWT Bearer | mesmas regras de `spaceKey` de `/chat` |
| `POST /ingest/markdown` | JWT Bearer | role `Admin` |
| `POST /ingest/confluence` | JWT Bearer | role `Admin` |
| `POST /reindex` | JWT Bearer | role `Admin` |
| `GET /documents`, `GET /documents/{id}` | JWT Bearer | qualquer role |
| `GET /spaces` | JWT Bearer | qualquer role (implementado dentro do `DocumentsController`) |
| `POST /feedback` | JWT Bearer | qualquer role |
| `GET /metrics` | nenhuma | formato texto Prometheus |

## POST /chat

Request:

```json
{
  "question": "Como funciona o fluxo de liquidacao?",
  "audience": "operations",
  "spaceKey": "OPS",
  "system": "settlement-service",
  "chatSessionId": null
}
```

Response (200):

```json
{
  "answer": "Resposta baseada em fontes recuperadas.",
  "domain": "business",
  "sources": [
    { "title": "Documento", "url": "docs/arquivo.md", "score": 0.42 }
  ],
  "evidenceStatus": "found",
  "confidence": 0.42
}
```

`confidence` é número (0 a 1). `sources[]` só tem `title`, `url` e `score` — não há `id`, `source`, `documentType` nem `content`. Se a evidência for insuficiente (melhor score < 0.15), `evidenceStatus` é `"insufficient"` e `confidence` é `0`. Pode retornar `403 Forbidden` se `spaceKey` não estiver entre os spaces permitidos do usuário.

## POST /auth/login

Request:

```json
{
  "email": "usuario@empresa.com",
  "password": "senha"
}
```

Response:

```json
{
  "accessToken": "jwt",
  "tokenType": "Bearer",
  "expiresIn": 3600
}
```

## POST /ingest/markdown

Request:

```json
{
  "rootDir": "docs",
  "spaceKey": "ENG"
}
```

Response:

```json
{
  "jobId": "uuid",
  "documentsProcessed": 10,
  "chunksProcessed": 42
}
```

## POST /mcp/search

Requer `X-Api-Key` e `Authorization: Bearer` (JWT de usuário) simultaneamente.

Request:

```json
{
  "query": "Qual endpoint consulta contratos?",
  "domain": "api",
  "audience": "developers",
  "spaceKey": "ENG",
  "system": null,
  "limit": 5
}
```

Response:

```json
{
  "query": "Qual endpoint consulta contratos?",
  "domain": "api",
  "results": [],
  "evidenceStatus": "insufficient"
}
```

`401` se faltar `X-Api-Key` ou o JWT; `403` se `spaceKey` não estiver entre os spaces permitidos do usuário do token.

## POST /feedback

Request:

```json
{
  "chatMessageId": "00000000-0000-0000-0000-000000000000",
  "helpful": true,
  "comment": "Resposta útil"
}
```

Response: `204 No Content`.

## Formato De Erro

```json
{ "error": "mensagem" }
```

Códigos usados: `400` (validação), `401` (não autenticado), `403` (autenticado mas sem permissão para o `spaceKey` pedido), `500` (erro inesperado).
