# Autenticação Da API

A API deve validar autenticação e autorização em todos os endpoints sensíveis.

## Opções Para O MVP

Opção corporativa preferencial:

```txt
External identity provider
  -> JWT
  -> API validates issuer, audience, signature and roles
```

Opção aceitável para MVP:

```txt
POST /auth/login
  -> API issues short-lived JWT
  -> frontend sends Authorization: Bearer
```

## Roles

```txt
admin
developer
operations
product
support
viewer
```

## Dimensões De Autorização

- role
- space
- audience
- documentType
- system
- source
