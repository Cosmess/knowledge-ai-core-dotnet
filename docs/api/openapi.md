# OpenAPI

A API NestJS expõe Swagger/OpenAPI em:

```txt
/docs
```

O repositório frontend deve gerar client e tipos a partir do contrato OpenAPI.

Ferramentas recomendadas:

```txt
openapi-typescript
openapi-fetch
```

Isso evita duplicação manual de DTOs entre `knowledge-ai-core` e `knowledge-ai-web`.
