# Dados Sensíveis

Nunca registrar em logs:

- access tokens
- refresh tokens
- API keys
- senhas
- segredos
- dados sensíveis brutos de clientes

Dados sensíveis devem ser mascarados antes de logs estruturados.

O package `observability` fornece helpers iniciais de mascaramento e deve ser expandido conforme tipos concretos de dados forem identificados.
