# Retrieval

## Implementado

Busca híbrida com reranking (`DocumentRepository.SearchAsync`):

```txt
1) candidatos: top (Limit x 5, mínimo 25) por similaridade de cosseno via pgvector (índice HNSW)
2) rerank: blend = 0.7 x score_vetorial + 0.3 x score_textual (ts_rank_cd sobre tsvector em português)
3) corte final: top Limit pelo blend
```

O score textual usa a coluna gerada `document_chunks.search_vector` (`tsvector`, `to_tsvector('portuguese', content)`, indexada via GIN) e `websearch_to_tsquery('portuguese', @QueryText)` sobre o texto da pergunta/consulta. Quando `QueryText` é vazio ou não há match textual, o score textual é `0` e o resultado equivale à busca puramente vetorial — não há regressão para o caso sem texto.

A estratégia de dois estágios (kNN amplo via índice + rerank do conjunto candidato) existe para não perder o uso do índice HNSW: ordenar diretamente por um score combinado impediria o planner de usar o índice na consulta inteira.

Filtros aplicados via SQL (todos opcionais — `null` = sem filtro):

- `domain`
- `audience`
- `spaceKey`
- `system`

Autorização: a busca é sempre restrita aos `AllowedSpaceKeys` do usuário autenticado (parâmetro `AllowedSpaceKeys` aplicado via `space_key = ANY(@AllowedSpaceKeys)`), além do filtro opcional `spaceKey` pedido pelo chamador. Se o `spaceKey` pedido não estiver entre os permitidos do usuário, a camada de aplicação rejeita a requisição com `403 Forbidden` antes mesmo de buscar.

Threshold de evidência: **0.15**, aplicado sobre o score combinado (blend) do melhor resultado — se for menor que isso, a busca é considerada `EvidenceStatus.Insufficient` (em `/chat`, o sistema não chama o LLM e devolve uma mensagem fixa; em `/mcp/search`, apenas reporta o status).

Limite de resultados: `MaxResults = 5` fixo em `/chat`; `Limit` configurável (1 a 50) em `/mcp/search`.

## Não Implementado

- filtros por `documentType`
- filtros por data de atualização/versão do documento
- semantic cache

Esses itens permanecem como possíveis evoluções futuras, sem implementação atual.
