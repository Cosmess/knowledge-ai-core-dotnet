# Docker

Serviços reais do `docker-compose.yml` (raiz do repositório):

```txt
postgres     # imagem pgvector/pgvector:pg16
redis
ollama
api          # KnowledgeAi.Api, Dockerfile próprio, porta 8080:8080
mcp          # KnowledgeAi.Mcp, Dockerfile próprio
prometheus   # imagem prom/prometheus, porta 9090:9090, scrape de api:8080/metrics
grafana      # imagem grafana/grafana, porta 3000:3000, datasource+dashboard provisionados
```

`api`, `mcp`, `prometheus` e `grafana` já estão containerizados — nenhum é mais "planejado".

Executar:

```bash
docker compose up -d
```

Variáveis de ambiente reais usadas pelos serviços `api`/`mcp` no compose incluem `Postgres__ConnectionString`, `Jwt__SigningKey`, `ApiKey__Value`, `AdminSeed__Email`/`AdminSeed__Password` (Api) e `KNOWLEDGE_API_KEY`, `KNOWLEDGE_USER_JWT` (Mcp — ver `docs/mcp/client-configuration.md`).

## Prometheus + Grafana

Configuração em `ops/`:

```txt
ops/prometheus/prometheus.yml                          # scrape config (target: api:8080)
ops/grafana/provisioning/datasources/datasource.yml     # datasource Prometheus, auto-provisionado
ops/grafana/provisioning/dashboards/dashboards.yml      # provider de dashboards (carrega arquivos do mesmo diretório)
ops/grafana/provisioning/dashboards/knowledge-ai-api.json  # dashboard básico (request rate, P95 latency, in-progress, GC)
```

Acesso: Prometheus em `http://localhost:9090`, Grafana em `http://localhost:3000` (login `admin` / senha em `GF_SECURITY_ADMIN_PASSWORD`, troque o placeholder antes de subir em produção).

## Seed De Usuário Admin

A Api cria automaticamente um usuário com role `Admin` no startup, caso ainda não exista um usuário com o e-mail configurado em `AdminSeed__Email` (idempotente — `ON CONFLICT (email) DO NOTHING`). Sem isso, um banco novo não teria nenhum usuário e seria impossível fazer login. Configure `AdminSeed__Email`/`AdminSeed__Password`/`AdminSeed__SpaceKeys__N` com valores reais antes de subir fora de dev local; deixando `Email`/`Password` vazios, a seed é simplesmente pulada.
