# OpenAPI

A API .NET (ASP.NET Core + Swashbuckle) expõe Swagger/OpenAPI em:

```txt
/swagger
```

Disponível apenas em ambiente Development (`app.Environment.IsDevelopment()`).

O contrato OpenAPI descreve os dois esquemas de segurança usados pela API: `Bearer` (JWT) e `ApiKey` (header `X-Api-Key`, usado por `/mcp/search` em conjunto com o Bearer — ver `docs/api/auth.md`).

Se um client/frontend precisar consumir essa API a partir de TypeScript, ferramentas como `openapi-typescript`/`openapi-fetch` continuam aplicáveis para gerar tipos a partir do contrato exposto em `/swagger`, evitando duplicação manual de DTOs.
