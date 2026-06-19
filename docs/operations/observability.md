# Observabilidade

## Implementado

- **Logs estruturados**: Serilog, com `SensitiveDataMaskingEnricher` já ativo no pipeline — mascara propriedades de log cujo nome contém password/token/apikey/secret/accesstoken/refreshtoken/authorization (ver `docs/security/sensitive-data.md`).
- **Tracing e métricas**: OpenTelemetry configurado para tracing e métricas de ASP.NET Core (latência por rota, status code, contagem de requisições). Hoje o exporter é só console — não há backend OTLP (Jaeger/Tempo/etc.) configurado ainda.
- **Métricas Prometheus**: endpoint `GET /metrics` real, via `prometheus-net.AspNetCore`, em formato texto Prometheus padrão.
- **Métricas de negócio do RAG**: uso de tokens, custo estimado, taxa de fallback e taxa de evidência insuficiente — ver `docs/operations/llmops.md`.
- **Grafana + Prometheus como serviços**: `docker-compose.yml` já sobe `prometheus` (scrape de `api:8080/metrics`, config em `ops/prometheus/prometheus.yml`) e `grafana` (datasource e dashboard provisionados automaticamente a partir de `ops/grafana/provisioning/`, dashboard `knowledge-ai-api.json` com taxa de requisições por status, latência P95, requisições em andamento e coleta de GC). Grafana fica em `http://localhost:3000` (usuário `admin`, senha em `GF_SECURITY_ADMIN_PASSWORD`).

## Ainda Pendente

- Backend de tracing distribuído (exporter OTLP) — hoje o tracing OpenTelemetry só vai para o console.
- Agregação de feedback (`POST /feedback` é persistido, mas sem dashboard/métrica agregada).
- Tokens/custo para o provedor Ollama (a API do SDK usado não expõe essa informação — ver `docs/operations/llmops.md`).

## O Que Monitorar

- latência de request (Grafana, painel "P95 request latency")
- taxa de erro por status code (Grafana, painel "Request rate by status code")
- uso de tokens e custo estimado por provedor de LLM (`llm_tokens_total`, `llm_cost_usd_total`)
- eventos de fallback (`llm_fallback_total`)
- taxa de evidência insuficiente (`chat_evidence_outcome_total`)
- feedback dos usuários (roadmap — ver acima)

Em todos os casos, segredos e dados sensíveis devem continuar passando pelo masking do Serilog antes de qualquer log estruturado ser emitido.
