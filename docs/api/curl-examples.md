# Exemplos Práticos Com cURL

Este guia mostra um fluxo mínimo para testar a API .NET localmente.

## 1. Subir Dependências

```bash
docker compose up -d
```

Serviços esperados:

```txt
PostgreSQL + pgvector: localhost:5432
Redis: localhost:6379
Ollama: localhost:11434
```

## 2. Rodar A API

```bash
dotnet run --project src/KnowledgeAi.Api
```

URL base (padrão local):

```txt
http://localhost:5080
```

No `docker-compose.yml`, a API é exposta em `8080:8080`.

Swagger (só em ambiente Development):

```txt
http://localhost:5080/swagger
```

## 3. Health Check

```bash
curl http://localhost:5080/health
```

Resposta esperada:

```json
{
  "status": "ok"
}
```

## 4. Login

```bash
curl -X POST http://localhost:5080/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"usuario@empresa.com","password":"senha"}'
```

Resposta:

```json
{
  "accessToken": "eyJ...",
  "tokenType": "Bearer",
  "expiresIn": 3600
}
```

No PowerShell:

```powershell
$login = Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:5080/auth/login" `
  -ContentType "application/json" `
  -Body '{"email":"usuario@empresa.com","password":"senha"}'

$token = $login.accessToken
```

No bash:

```bash
TOKEN=$(curl -s -X POST http://localhost:5080/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"usuario@empresa.com","password":"senha"}' | jq -r '.accessToken')
```

## 5. Usuário Atual

```bash
curl http://localhost:5080/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

## 6. Ingestão Markdown (requer role Admin)

```bash
curl -X POST http://localhost:5080/ingest/markdown \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"rootDir":"docs","spaceKey":"ENG"}'
```

Resposta:

```json
{
  "jobId": "uuid",
  "documentsProcessed": 10,
  "chunksProcessed": 42
}
```

Sem role `Admin`, a resposta é `403 Forbidden`.

## 7. Ingestão Confluence (requer role Admin)

```bash
curl -X POST http://localhost:5080/ingest/confluence \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"spaceKey":"ENG"}'
```

## 8. Chat

Requer JWT válido. Se o `spaceKey` pedido não estiver entre os `allowedSpaceKeys` do usuário, a resposta é `403 Forbidden`.

```bash
curl -X POST http://localhost:5080/chat \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"question":"Como funciona o fluxo de liquidação?","audience":"developers","spaceKey":"ENG","system":"settlement-service"}'
```

Resposta (evidência suficiente):

```json
{
  "answer": "Resposta baseada nos trechos recuperados...",
  "domain": "business",
  "sources": [
    {
      "title": "Documento",
      "url": "docs/arquivo.md",
      "score": 0.82
    }
  ],
  "evidenceStatus": "found",
  "confidence": 0.82
}
```

Quando não houver evidência suficiente (score do melhor resultado abaixo de 0.15):

```json
{
  "answer": "Não há evidência suficiente nas fontes recuperadas para responder com confiança.",
  "domain": "technical",
  "sources": [],
  "evidenceStatus": "insufficient",
  "confidence": 0
}
```

`confidence` é sempre um número (0 a 1), não uma string `"high"`/`"low"`.

## 9. Busca MCP Pela API

Esse endpoint é chamado pelo servidor MCP. Exige `X-Api-Key` **e** `Authorization: Bearer` (JWT de usuário) ao mesmo tempo — faltando qualquer um dos dois, a resposta é `401`.

```bash
curl -X POST http://localhost:5080/mcp/search \
  -H "Content-Type: application/json" \
  -H "X-Api-Key: $API_KEY" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"query":"Qual endpoint consulta contratos?","domain":"api","audience":"developers","spaceKey":"ENG","limit":5}'
```

Resposta:

```json
{
  "query": "Qual endpoint consulta contratos?",
  "domain": "api",
  "results": [],
  "evidenceStatus": "insufficient"
}
```

Se `spaceKey` não estiver entre os `allowedSpaceKeys` do usuário do token, a resposta é `403 Forbidden`.

## 10. Listar Documentos

```bash
curl http://localhost:5080/documents \
  -H "Authorization: Bearer $TOKEN"
```

## 11. Listar Spaces

```bash
curl http://localhost:5080/spaces \
  -H "Authorization: Bearer $TOKEN"
```

## 12. Feedback

```bash
curl -X POST http://localhost:5080/feedback \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"chatMessageId":"00000000-0000-0000-0000-000000000000","helpful":true,"comment":"Resposta útil"}'
```

Resposta: `204 No Content`.

## 13. Métricas

```bash
curl http://localhost:5080/metrics
```

Resposta em formato texto Prometheus (via `prometheus-net`), não JSON:

```txt
# HELP http_requests_received_total ...
# TYPE http_requests_received_total counter
http_requests_received_total{...} 1
...
```

## 14. Rodar O MCP Localmente

```bash
KNOWLEDGE_API_BASE_URL=http://localhost:5080 \
KNOWLEDGE_API_KEY=<api-key> \
KNOWLEDGE_USER_JWT=$TOKEN \
dotnet run --project src/KnowledgeAi.Mcp
```

As três variáveis são obrigatórias — o processo falha ao iniciar se `KNOWLEDGE_API_KEY` ou `KNOWLEDGE_USER_JWT` estiverem ausentes. `KNOWLEDGE_USER_JWT` é o mesmo JWT obtido em `POST /auth/login` (passo 4); quando expirar, gere um novo e reinicie o cliente MCP. Veja `docs/mcp/client-configuration.md` para exemplos de configuração em clientes como Claude Desktop, Cursor e VS Code.
