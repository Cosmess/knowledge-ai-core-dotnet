# Dados Sensíveis

Nunca registrar em logs:

- access tokens
- refresh tokens
- API keys
- senhas
- segredos

Esse mascaramento já está implementado e ativo: o pipeline de logging (Serilog) usa um `SensitiveDataMaskingEnricher` que mascara propriedades de log cujo nome contém `password`, `token`, `apikey`, `secret`, `accesstoken`, `refreshtoken` ou `authorization`, antes de qualquer log estruturado ser emitido — não é mais um helper desconectado do pipeline.

## Limitação Conhecida

O mascaramento atual age só pelo **nome** da propriedade de log. Ele não faz DLP de conteúdo: não detecta nem mascara dados sensíveis (CPF, e-mail, etc.) que apareçam dentro de texto livre/valores de propriedades cujo nome não bate com a lista acima. Se um caso assim for identificado, o enricher precisa ser expandido com regras adicionais por padrão de valor, não só por nome.
