# Endpoints Da API

## Implementados No Scaffold

```http
GET /health
POST /auth/login
GET /auth/me
POST /chat
POST /mcp/search
POST /ingest/markdown
POST /ingest/confluence
POST /reindex
GET /documents
GET /documents/:id
GET /spaces
POST /feedback
GET /metrics
```

## POST /chat

Request:

```json
{
  "question": "Como funciona o fluxo de liquidacao?",
  "audience": "operations",
  "spaceKey": "OPS",
  "system": "settlement-service"
}
```

Response:

```json
{
  "answer": "Resposta baseada em fontes recuperadas.",
  "domain": "business_rule",
  "sources": [],
  "confidence": "low"
}
```

## POST /auth/login

Request:

```json
{
  "email": "admin@example.com",
  "password": "admin"
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

Request:

```json
{
  "query": "Qual endpoint consulta contratos?",
  "domain": "api_documentation",
  "audience": "developers",
  "spaceKey": "ENG",
  "limit": 5
}
```

Response:

```json
{
  "query": "Qual endpoint consulta contratos?",
  "domain": "api_documentation",
  "results": [],
  "evidenceStatus": "insufficient"
}
```
