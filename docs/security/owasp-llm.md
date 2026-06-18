# Controles OWASP LLM

Riscos:

- prompt injection
- vazamento de dados sensíveis
- uso indevido de tools
- autonomia excessiva
- alucinação
- tratamento inseguro de saída
- contexto recuperado não confiável

Controles:

- usar apenas contexto recuperado e autorizado
- citar fontes
- recusar quando a evidência for insuficiente
- sanitizar documentos ingeridos
- mascarar logs sensíveis
- validar entradas
- limitar respostas das tools MCP
- nunca expor credenciais ou tokens
