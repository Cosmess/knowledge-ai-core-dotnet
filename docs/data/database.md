# Banco De Dados

Banco do MVP: PostgreSQL com pgvector.

Tabelas:

```sql
documents
document_chunks
chat_sessions
chat_messages
feedbacks
prompt_versions
ingestion_jobs
```

## document_chunks

```sql
create table document_chunks (
  id uuid primary key,
  document_id uuid not null,
  content text not null,
  embedding vector,
  metadata jsonb not null,
  created_at timestamp not null default now(),
  updated_at timestamp not null default now()
);
```
