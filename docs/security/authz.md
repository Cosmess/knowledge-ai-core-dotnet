# Autorização

A autorização é aplicada na API .NET, não no cliente.

## Dimensões

- **role** — aplicado: role `Admin` é exigida para `/ingest/markdown`, `/ingest/confluence` e `/reindex`. Os demais endpoints autenticados aceitam qualquer role.
- **spaceKey** — aplicado: cada usuário tem `AllowedSpaceKeys` (coluna `allowed_space_keys` na tabela `users`, propagada como claims `space` no JWT). `/chat` e `/mcp/search` verificam que o `spaceKey` pedido está nessa lista; caso contrário, `403 Forbidden`. Se `/mcp/search` for chamado sem `spaceKey`, a busca é restrita automaticamente aos spaces permitidos do usuário.
- **audience**, **documentType**, **system** — são apenas filtros de busca aceitos no request, não dimensões de autorização aplicadas pelo backend.
- **user**, **tenant/company** — não há conceito de tenant/empresa nas entidades atuais; a granularidade de autorização é por usuário individual.

## Exemplos

`Developer` pode acessar documentação técnica e de API dos spaces permitidos (`AllowedSpaceKeys`).

`Operations`/`Product`/`Support`/`Viewer` podem acessar `/chat`, `/documents`, `/feedback` normalmente, sujeitos à mesma restrição de `spaceKey`.

`Admin` pode disparar ingestão e reindexação (`/ingest/markdown`, `/ingest/confluence`, `/reindex`). Isso não inclui acesso automático a todos os spaces em `/chat`/`/mcp/search` — um admin que precise consultar todos os spaces precisa ser provisionado com a lista completa em `allowed_space_keys`, igual qualquer outro usuário. Essa é uma decisão deliberada para evitar um caminho de escalonamento de privilégio implícito.
