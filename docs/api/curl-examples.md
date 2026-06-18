# Exemplos Práticos Com cURL

Este guia mostra um fluxo mínimo para testar a API localmente.

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
pnpm dev:api
```

URL base:

```txt
http://localhost:3000
```

Swagger:

```txt
http://localhost:3000/docs
```

## 3. Health Check

```bash
curl http://localhost:3000/health
```

Resposta esperada:

```json
{
  "status": "ok",
  "service": "knowledge-api",
  "timestamp": "2026-06-03T00:00:00.000Z"
}
```

## 4. Login

Usuário padrão de desenvolvimento:

```txt
email: admin@example.com
password: admin
```

```bash
curl -X POST http://localhost:3000/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"admin@example.com\",\"password\":\"admin\"}"
```

Resposta:

```json
{
  "accessToken": "eyJ...",
  "tokenType": "Bearer",
  "expiresIn": 3600
}
```

No PowerShell, guarde o token assim:

```powershell
$login = Invoke-RestMethod `
  -Method Post `
  -Uri "http://localhost:3000/auth/login" `
  -ContentType "application/json" `
  -Body '{"email":"admin@example.com","password":"admin"}'

$token = $login.accessToken
```

No bash:

```bash
TOKEN=$(curl -s -X POST http://localhost:3000/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin@example.com","password":"admin"}' | jq -r '.accessToken')
```

## 5. Usuário Atual

PowerShell:

```powershell
curl.exe http://localhost:3000/auth/me `
  -H "Authorization: Bearer $token"
```

Bash:

```bash
curl http://localhost:3000/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

## 6. Ingestão Markdown

Por padrão, a API lê a pasta configurada em `MARKDOWN_DOCS_ROOT`.

Também é possível informar `rootDir` no request:

PowerShell:

```powershell
curl.exe -X POST http://localhost:3000/ingest/markdown `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer $token" `
  -d "{\"rootDir\":\"docs\",\"spaceKey\":\"ENG\"}"
```

Bash:

```bash
curl -X POST http://localhost:3000/ingest/markdown \
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

## 7. Ingestão Confluence

Antes, configure:

```env
CONFLUENCE_BASE_URL=https://empresa.atlassian.net/wiki
CONFLUENCE_EMAIL=usuario@empresa.com
CONFLUENCE_API_TOKEN=token
CONFLUENCE_DEFAULT_SPACE=ENG
```

PowerShell:

```powershell
curl.exe -X POST http://localhost:3000/ingest/confluence `
  -H "Content-Type: application/json" `
  -H "Authorization: Bearer $token" `
  -d "{\"spaceKey\":\"ENG\"}"
```

Bash:

```bash
curl -X POST http://localhost:3000/ingest/confluence \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{"spaceKey":"ENG"}'
```

## 8. Chat

PowerShell:

```powershell
curl.exe -X POST http://localhost:3000/chat `
  -H "Content-Type: application/json" `
  -d "{\"question\":\"Como funciona o fluxo de liquidação?\",\"audience\":\"developers\",\"spaceKey\":\"ENG\"}"
```

Bash:

```bash
curl -X POST http://localhost:3000/chat \
  -H "Content-Type: application/json" \
  -d '{"question":"Como funciona o fluxo de liquidação?","audience":"developers","spaceKey":"ENG"}'
```

Resposta:

```json
{
  "answer": "Resposta baseada nos trechos recuperados...",
  "domain": "business_rule",
  "sources": [
    {
      "id": "uuid",
      "title": "Documento",
      "source": "markdown",
      "url": "C:\\projetos\\knowledge-ai-core\\docs\\arquivo.md",
      "documentType": "technical_doc",
      "content": "Trecho recuperado...",
      "score": 0.82
    }
  ],
  "confidence": "high"
}
```

Quando não houver evidência suficiente:

```json
{
  "answer": "Não encontrei evidência suficiente na base indexada para responder com segurança...",
  "domain": "technical",
  "sources": [],
  "confidence": "low"
}
```

## 9. Busca MCP Pela API

Esse endpoint é usado pelo MCP Server.

PowerShell:

```powershell
curl.exe -X POST http://localhost:3000/mcp/search `
  -H "Content-Type: application/json" `
  -d "{\"query\":\"Qual endpoint consulta contratos?\",\"domain\":\"api_documentation\",\"audience\":\"developers\",\"spaceKey\":\"ENG\",\"limit\":5}"
```

Bash:

```bash
curl -X POST http://localhost:3000/mcp/search \
  -H "Content-Type: application/json" \
  -d '{"query":"Qual endpoint consulta contratos?","domain":"api_documentation","audience":"developers","spaceKey":"ENG","limit":5}'
```

Resposta:

```json
{
  "query": "Qual endpoint consulta contratos?",
  "domain": "api_documentation",
  "results": [],
  "evidenceStatus": "insufficient"
}
```

## 10. Listar Documentos

PowerShell:

```powershell
curl.exe http://localhost:3000/documents `
  -H "Authorization: Bearer $token"
```

Bash:

```bash
curl http://localhost:3000/documents \
  -H "Authorization: Bearer $TOKEN"
```

## 11. Listar Spaces

PowerShell:

```powershell
curl.exe http://localhost:3000/spaces `
  -H "Authorization: Bearer $token"
```

Bash:

```bash
curl http://localhost:3000/spaces \
  -H "Authorization: Bearer $TOKEN"
```

## 12. Feedback

PowerShell:

```powershell
curl.exe -X POST http://localhost:3000/feedback `
  -H "Content-Type: application/json" `
  -d "{\"question\":\"Como funciona o fluxo?\",\"answer\":\"Resposta recebida\",\"useful\":true,\"comment\":\"Resposta útil\"}"
```

Bash:

```bash
curl -X POST http://localhost:3000/feedback \
  -H "Content-Type: application/json" \
  -d '{"question":"Como funciona o fluxo?","answer":"Resposta recebida","useful":true,"comment":"Resposta útil"}'
```

## 13. Métricas

```bash
curl http://localhost:3000/metrics
```

Resposta:

```json
{
  "requests": 1,
  "answered": 1,
  "insufficientEvidence": 0,
  "fallbacks": 1,
  "totalLatencyMs": 120,
  "averageLatencyMs": 120
}
```

## 14. Rodar O MCP Localmente

Com a API rodando:

```bash
pnpm dev:mcp
```

Ou via cliente MCP:

```bash
pnpm --dir C:\projetos\knowledge-ai-core dev:mcp
```

Exemplos de configuração:

```txt
.codex/config.toml.example
.cursor/mcp.json.example
.vscode/mcp.json.example
```
