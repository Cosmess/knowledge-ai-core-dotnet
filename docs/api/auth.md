# Autenticação Da API

A API valida autenticação e autorização em todos os endpoints sensíveis. Hoje existem dois esquemas, usados em contextos diferentes.

## Esquema 1 — JWT Bearer (usuários)

```txt
POST /auth/login {email, password}
  -> API emite um JWT de curta duração
  -> {accessToken, tokenType, expiresIn}
  -> cliente envia Authorization: Bearer <accessToken> nas chamadas seguintes
```

Usado por: `/chat`, `/auth/me`, `/documents`, `/documents/{id}`, `/spaces`, `/feedback`, `/ingest/markdown`, `/ingest/confluence`, `/reindex`.

O JWT carrega claims de `role` e de `space` (uma claim por space permitido — ver `AllowedSpaceKeys` abaixo). Validação no servidor: emissor, audiência, assinatura (HMAC-SHA256) e validade.

## Esquema 2 — API Key + JWT (MCP)

`POST /mcp/search` é chamado pelo servidor MCP, não diretamente por um usuário em um browser. Esse endpoint exige **os dois mecanismos ao mesmo tempo**:

```txt
header X-Api-Key: <chave fixa do processo MCP>
  -> prova que a chamada vem do binário MCP confiável
header Authorization: Bearer <JWT do usuário final>
  -> identifica QUEM está usando o MCP, para aplicar AllowedSpaceKeys
```

Se faltar a API Key ou o JWT (ou o JWT estiver expirado/inválido), a resposta é `401 Unauthorized`. Esse JWT é obtido da mesma forma que qualquer outro client: via `POST /auth/login`.

## Roles

```txt
Admin
Developer
Operations
Product
Support
Viewer
```

No código é um enum C# (`Role`), serializado em JSON como camelCase (`"developer"`, `"admin"`, etc). Hoje só uma regra de role é de fato aplicada: `Admin` é exigido para `/ingest/markdown`, `/ingest/confluence` e `/reindex`. Os demais endpoints autenticados aceitam qualquer role.

## Dimensões De Autorização

- **role** — aplicado (admin-only em ingestão/reindexação).
- **spaceKey** — aplicado: o `spaceKey` pedido em `/chat` e `/mcp/search` precisa estar entre os `AllowedSpaceKeys` do usuário autenticado, senão a API responde `403 Forbidden`. Se `/mcp/search` for chamado sem `spaceKey`, a busca é restrita automaticamente aos spaces permitidos do usuário (nunca busca em todos os spaces sem filtro).
- **audience**, **documentType**, **system** — são apenas filtros de busca aceitos no request; não são dimensões de autorização impostas pelo backend.

Não há bypass automático de `AllowedSpaceKeys` para a role `Admin`: um admin que precise consultar todos os spaces precisa ser provisionado com a lista completa em `allowed_space_keys`, igual qualquer outro usuário.
