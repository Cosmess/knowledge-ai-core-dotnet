# Controles OWASP LLM

Riscos:

- prompt injection
- vazamento de dados sensíveis
- uso indevido de tools
- autonomia excessiva
- alucinação
- tratamento inseguro de saída
- contexto recuperado não confiável

Controles e status real de implementação:

- **usar apenas contexto recuperado** — implementado: o system prompt do chat instrui a usar só o contexto recuperado; não há mecanismo de busca externa.
- **recusar quando a evidência for insuficiente** — implementado: threshold de similaridade 0.15 (`EvidenceThreshold`); abaixo disso, `/chat` e `/mcp/search` reportam `evidenceStatus: insufficient` em vez de responder com confiança.
- **citar fontes** — parcialmente implementado: a resposta de `/chat` inclui `sources[]` (título, URL, score); o system prompt instrui a citar fontes, mas o texto do contexto enviado ao LLM não inclui título/URL explicitamente — a citação depende do próprio conteúdo do chunk.
- **autorizar o contexto usado** — implementado desde esta correção: a busca é sempre restrita aos `AllowedSpaceKeys` do usuário autenticado (`/chat` e `/mcp/search`); pedir um `spaceKey` fora dessa lista retorna `403 Forbidden`. `audience`/`documentType`/`system` continuam sendo filtros, não autorização.
- **sanitizar documentos ingeridos** — implementado com escopo limitado: conteúdo ingerido (Markdown e HTML do Confluence) passa por um sanitizador que remove tags `<script>`, `<style>`, `<iframe>`, `<object>`, `<embed>`, `<noscript>` (com seu conteúdo interno) e atributos `on*=`/`javascript:`, antes do chunking. Isso mitiga injeção de markup/script no contexto do LLM. **Não** mitiga prompt-injection em linguagem natural (instruções maliciosas escritas como texto normal dentro de um documento) — esse risco continua dependendo só das regras do system prompt (ex.: "nunca exponha segredos", "use apenas o contexto recuperado").
- **mascarar logs sensíveis** — implementado e ativo (ver `docs/security/sensitive-data.md`).
- **validar entradas** — implementado via FluentValidation em todos os commands/queries.
- **limitar respostas das tools MCP** — parcialmente implementado: o parâmetro `limit` (quantidade de resultados) é respeitado; não há limite de tamanho/conteúdo do payload retornado por cada resultado.
- **nunca expor credenciais ou tokens** — coberto pelo masking de logs e pela instrução explícita no system prompt; não há um filtro de saída do LLM que verifique isso de forma independente.
