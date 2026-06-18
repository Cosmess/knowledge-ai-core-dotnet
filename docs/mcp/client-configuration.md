# Configuração De Clientes MCP

O `dev-knowledge-mcp` usa transporte `stdio`, então pode ser conectado a clientes MCP que executam um comando local.

Antes de conectar qualquer cliente, a API precisa estar rodando:

```bash
pnpm dev:api
```

Depois, o cliente MCP pode iniciar o servidor com:

```bash
pnpm --dir C:\projetos\knowledge-ai-core dev:mcp
```

Variável necessária:

```env
KNOWLEDGE_API_BASE_URL=http://localhost:3000
```

## Codex

Exemplo disponível em:

```txt
.codex/config.toml.example
```

Conteúdo:

```toml
[mcp_servers.dev-knowledge-mcp]
command = "pnpm"
args = ["--dir", "C:\\projetos\\knowledge-ai-core", "dev:mcp"]

[mcp_servers.dev-knowledge-mcp.env]
KNOWLEDGE_API_BASE_URL = "http://localhost:3000"
```

Use esse conteúdo em `.codex/config.toml` quando quiser habilitar o MCP neste repositório.

## Cursor

Exemplo disponível em:

```txt
.cursor/mcp.json.example
```

Conteúdo:

```json
{
  "mcpServers": {
    "dev-knowledge-mcp": {
      "command": "pnpm",
      "args": ["--dir", "C:\\projetos\\knowledge-ai-core", "dev:mcp"],
      "env": {
        "KNOWLEDGE_API_BASE_URL": "http://localhost:3000"
      }
    }
  }
}
```

## VS Code

Exemplo disponível em:

```txt
.vscode/mcp.json.example
```

Conteúdo:

```json
{
  "servers": {
    "dev-knowledge-mcp": {
      "type": "stdio",
      "command": "pnpm",
      "args": ["--dir", "C:\\projetos\\knowledge-ai-core", "dev:mcp"],
      "env": {
        "KNOWLEDGE_API_BASE_URL": "http://localhost:3000"
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

## Limitações Atuais

O servidor MCP já conecta por `stdio` e chama a Knowledge API em `/mcp/search`.

Ainda falta:

- autenticação entre MCP e API;
- retrieval real com pgvector;
- documentos indexados;
- retorno de fontes reais;
- empacotamento binário publicado;
- testes com clientes MCP reais.
