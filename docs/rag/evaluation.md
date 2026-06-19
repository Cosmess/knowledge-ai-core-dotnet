# Avaliação

## Checagens Implementadas

- a resposta recusa quando a evidência é insuficiente (threshold de similaridade 0.15 sobre o melhor resultado — `EvidenceStatus.Insufficient`)
- a resposta só usa contexto de spaces que o usuário autenticado tem permissão de acessar (`AllowedSpaceKeys`, aplicado em `/chat` e `/mcp/search`)
- a busca é restrita pelo `domain` classificado da pergunta (classificação por palavras-chave, `KeywordQuestionClassifier`)

## Checagens Ainda Não Implementadas (roadmap)

- verificação automática de que a resposta de fato cita as fontes corretamente (hoje depende só da instrução do system prompt, sem checagem programática)
- detecção de exposição de segredos na resposta (depende só da instrução do system prompt)
- avaliação de qualidade de resposta com golden set / LLM-as-judge

## Métricas

Implementadas:

- **Score de retrieval** — similaridade de cosseno por resultado (0 a 1).
- **Confidence** — igual ao score do melhor resultado quando há evidência suficiente; `0` quando não há.

Ainda não implementadas (roadmap):

- feedback do usuário agregado (existe `POST /feedback`, salvo no banco, mas sem agregação/dashboard)
- taxa de perguntas sem resposta (evidência insuficiente)
- taxa de fallback (resposta extrativa quando o LLM falha)
- latência por etapa
- uso de tokens por chamada de LLM
