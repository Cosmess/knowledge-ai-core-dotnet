# Banco De Dados

Banco: PostgreSQL com extensão pgvector (imagem `pgvector/pgvector:pg16`).

O schema é aplicado por um script SQL idempotente (`Schema.sql`, em `src/KnowledgeAi.Infrastructure/Persistence/Migrations`), executado no startup da aplicação por um `DatabaseInitializer`. Não são migrations do EF Core nem migrations versionadas incrementais — é um conjunto de `create table if not exists` / `create index if not exists` reaplicado a cada inicialização.

Tabelas:

```sql
users
documents
document_chunks
chat_sessions
chat_messages
feedbacks
prompt_versions
ingestion_jobs
```

## users

```sql
create table if not exists users (
    id uuid primary key,
    email text not null unique,
    password_hash text not null,
    role text not null,
    allowed_space_keys text[] not null default '{}',
    created_at timestamptz not null default now()
);
```

`allowed_space_keys` é a lista de `spaceKey` que o usuário está autorizado a consultar; é usada para autorização em `/chat` e em `/mcp/search`.

## documents

```sql
create table if not exists documents (
    id uuid primary key,
    title text not null,
    url text not null unique,
    source text not null,
    space_key text not null,
    document_type text not null,
    audience text not null,
    system text not null,
    version integer not null,
    updated_at timestamptz not null,
    created_at timestamptz not null default now()
);
```

Os metadados de classificação (`source`, `space_key`, `document_type`, `audience`, `system`, `title`, `url`, `version`, `updated_at`) são colunas relacionais desta tabela — não um blob JSON.

## document_chunks

```sql
create table if not exists document_chunks (
    id uuid primary key,
    document_id uuid not null references documents(id) on delete cascade,
    content text not null,
    embedding vector(1536) not null,
    domain text not null,
    metadata jsonb not null default '{}',
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

alter table document_chunks
    add column if not exists search_vector tsvector generated always as (to_tsvector('portuguese', content)) stored;

create index if not exists document_chunks_embedding_idx
    on document_chunks using hnsw (embedding vector_cosine_ops);

create index if not exists document_chunks_search_vector_idx
    on document_chunks using gin (search_vector);
```

A dimensão do vetor é fixa em 1536 para casar com o modelo de embedding `text-embedding-3-small`. Trocar o provedor de embedding ativo por um com dimensão diferente exige migrar esta coluna (e os dados existentes).

O índice vetorial é um índice HNSW com `vector_cosine_ops`, já criado no schema — não há etapa de indexação separada após o `insert`.

`search_vector` é uma coluna gerada (`GENERATED ALWAYS AS ... STORED`) usada pela busca híbrida (ver `docs/rag/retrieval.md`): combina o score vetorial com `ts_rank_cd` sobre esse `tsvector`, indexado via GIN.

A coluna `metadata` (jsonb) é o único metadado em formato JSON real do modelo de dados, e na prática contém apenas a chave `headingPath` (caminho hierárquico do heading do chunk, ex.: `"Título > Subtítulo"`).

## chat_sessions / chat_messages

```sql
create table if not exists chat_sessions (
    id uuid primary key,
    user_id uuid not null,
    created_at timestamptz not null default now()
);

create table if not exists chat_messages (
    id uuid primary key,
    chat_session_id uuid not null references chat_sessions(id) on delete cascade,
    question text not null,
    answer text not null,
    domain text not null,
    evidence_status text not null,
    confidence double precision not null,
    created_at timestamptz not null default now()
);
```

## feedbacks

```sql
create table if not exists feedbacks (
    id uuid primary key,
    chat_message_id uuid not null references chat_messages(id) on delete cascade,
    user_id uuid not null,
    helpful boolean not null,
    comment text,
    created_at timestamptz not null default now()
);
```

## prompt_versions

```sql
create table if not exists prompt_versions (
    id uuid primary key,
    name text not null,
    audience text not null,
    domain text not null,
    template text not null,
    version integer not null,
    is_active boolean not null,
    created_at timestamptz not null default now()
);
```

## ingestion_jobs

```sql
create table if not exists ingestion_jobs (
    id uuid primary key,
    source text not null,
    space_key text not null,
    status text not null,
    documents_processed integer not null default 0,
    chunks_processed integer not null default 0,
    error text,
    started_at timestamptz not null default now(),
    completed_at timestamptz
);
```

## Acesso a dados

O acesso às tabelas acima é feito via Dapper + Npgsql, através de repositórios na camada `KnowledgeAi.Infrastructure` (`DocumentRepository`, `UserRepository`, etc.), sem ORM completo (sem EF Core) e sem camada de abstração para outros vector stores.
