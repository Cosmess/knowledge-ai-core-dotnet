# Configuração De Clientes MCP

O servidor MCP (`KnowledgeAi.Mcp`, .NET) usa transporte `stdio`, então pode ser conectado a clientes MCP que executam um comando local.

Antes de conectar qualquer cliente, a API precisa estar rodando:

```bash
dotnet run --project src/KnowledgeAi.Api
```

Depois, o cliente MCP pode iniciar o servidor com:

```bash
dotnet run --project src/KnowledgeAi.Mcp
```

Variáveis necessárias:

```env
KNOWLEDGE_API_BASE_URL=http://localhost:5080
KNOWLEDGE_API_KEY=<chave fixa do processo MCP>
KNOWLEDGE_USER_JWT=<JWT do usuário, obtido via POST /auth/login>
```

`KNOWLEDGE_API_BASE_URL` é opcional (default `http://localhost:5080`). `KNOWLEDGE_API_KEY` e `KNOWLEDGE_USER_JWT` são **obrigatórias** — o processo falha ao iniciar se qualquer uma estiver ausente.

`KNOWLEDGE_USER_JWT` identifica o usuário final por trás do processo MCP, permitindo que a API aplique `AllowedSpaceKeys` por usuário nas buscas via MCP (sem ele, o MCP só teria uma identidade de serviço compartilhada via API Key, sem usuário real associado). Esse JWT expira conforme `Jwt:ExpiresInSeconds` configurado na API; não há renovação automática — quando expirar, gere um novo token (`POST /auth/login`) e reinicie o cliente MCP com a variável atualizada.

## Codex

Adicione ao `.codex/config.toml`:

```toml
[mcp_servers.knowledge-ai-mcp]
command = "dotnet"
args = ["run", "--project", "C:\\projetos\\knowledge-ai-core-dotnet\\src\\KnowledgeAi.Mcp"]

[mcp_servers.knowledge-ai-mcp.env]
KNOWLEDGE_API_BASE_URL = "http://localhost:5080"
KNOWLEDGE_API_KEY = "<api-key>"
KNOWLEDGE_USER_JWT = "<jwt-do-usuario>"
```

## Cursor

Adicione ao `.cursor/mcp.json`:

```json
{
  "mcpServers": {
    "knowledge-ai-mcp": {
      "command": "dotnet",
      "args": ["run", "--project", "C:\\projetos\\knowledge-ai-core-dotnet\\src\\KnowledgeAi.Mcp"],
      "env": {
        "KNOWLEDGE_API_BASE_URL": "http://localhost:5080",
        "KNOWLEDGE_API_KEY": "<api-key>",
        "KNOWLEDGE_USER_JWT": "<jwt-do-usuario>"
      }
    }
  }
}
```

## VS Code

Adicione ao `.vscode/mcp.json`:

```json
{
  "servers": {
    "knowledge-ai-mcp": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["run", "--project", "C:\\projetos\\knowledge-ai-core-dotnet\\src\\KnowledgeAi.Mcp"],
      "env": {
        "KNOWLEDGE_API_BASE_URL": "http://localhost:5080",
        "KNOWLEDGE_API_KEY": "<api-key>",
        "KNOWLEDGE_USER_JWT": "<jwt-do-usuario>"
      }
    }
  }
}
```

## Tools Expostas

```txt
search_technical_docs
search_business_rules
search_api_docs
search_architecture_docs
search_user_stories
get_service_context
```

Veja `docs/mcp/tools.md` para o schema de cada uma.
