# LLMOps

## Status Real

- **Versionamento de prompts**: existe uma tabela `prompt_versions` no schema do banco, mas nenhum código a usa ativamente hoje — o system prompt do chat é fixo no código (`AskQuestionCommandHandler.BuildSystemPrompt`), com duas variantes por `Audience`. Versionamento em runtime é roadmap, não implementado.
- **Modelos de embedding/chat**: configuráveis via `LlmProviderOptions` (OpenAI, Anthropic, Ollama), mas não versionados/registrados por resposta — não há metadado salvo dizendo "essa resposta usou o modelo X na versão Y".
- **Estratégia de chunking/retrieval**: fixa em constantes no código (`WordBudget = 450`, `MinWordsPerSection = 30`, `EvidenceThreshold = 0.15`, pesos do blend híbrido `0.7`/`0.3`), não configurável em runtime nem versionada.

## Métricas Reais Vs. Roadmap

Implementado, exposto em `/metrics` (Prometheus) via `PrometheusLlmMetricsRecorder`:

- `Score` por resultado de busca (score combinado vetor+texto, 0 a 1) e `Confidence` por resposta de chat.
- `llm_tokens_total{provider, direction="input"|"output"}` — tokens de entrada/saída por chamada de LLM. Reportado para OpenAI e Anthropic (os SDKs expõem `Usage` na resposta); **não reportado para Ollama** hoje, porque o helper de alto nível usado (`OllamaSharp.Chat`) não repassa `prompt_eval_count`/`eval_count` da API nativa do Ollama.
- `llm_cost_usd_total{provider}` — custo estimado, calculado a partir de `LlmPricing:Providers:<provider>` (`appsettings.json`/env vars). **Fica em 0 se não configurado** — não há tabela de preço hardcoded (preços de provedores mudam; um valor chumbado ficaria errado silenciosamente).
- `llm_fallback_total{provider}` — quantas vezes a resposta extrativa foi usada porque a chamada ao LLM falhou.
- `chat_evidence_outcome_total{outcome="found"|"insufficient"}` — taxa de perguntas/buscas com evidência suficiente vs. insuficiente, registrado tanto em `/chat` quanto em `/mcp/search`.
- Métricas HTTP padrão via OpenTelemetry/Prometheus (latência por rota, status code, contagem de requisições) — ver `docs/operations/observability.md`.

Ainda não implementado (roadmap):

- score de feedback agregado (existe `POST /feedback`, persistido, mas sem agregação/dashboard)
- tokens/custo para Ollama (ver limitação acima)
- latência por etapa do pipeline RAG (embedding, busca, geração) separadamente — hoje só a latência HTTP total por rota é medida

Quando o feedback agregado e a granularidade por etapa forem implementados, devem ser suficientes para reproduzir por que uma resposta foi gerada (modelo usado, score dos chunks recuperados, versão do prompt).
