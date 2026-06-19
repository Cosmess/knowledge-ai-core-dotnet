create extension if not exists vector;

-- Embedding dimension is fixed at 1536 to match OpenAI text-embedding-3-small.
-- If you swap the active embedding provider for one with a different dimension,
-- this column (and any existing data) must be migrated accordingly.

create table if not exists users (
    id uuid primary key,
    email text not null unique,
    password_hash text not null,
    role text not null,
    allowed_space_keys text[] not null default '{}',
    created_at timestamptz not null default now()
);

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

-- Generated column for hybrid search (vector similarity + full-text rank). Added via ALTER
-- (rather than inline in the CREATE TABLE above) so it also backfills databases created
-- before hybrid search existed.
alter table document_chunks
    add column if not exists search_vector tsvector generated always as (to_tsvector('portuguese', content)) stored;

create index if not exists document_chunks_embedding_idx
    on document_chunks using hnsw (embedding vector_cosine_ops);

create index if not exists document_chunks_search_vector_idx
    on document_chunks using gin (search_vector);

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

create table if not exists feedbacks (
    id uuid primary key,
    chat_message_id uuid not null references chat_messages(id) on delete cascade,
    user_id uuid not null,
    helpful boolean not null,
    comment text,
    created_at timestamptz not null default now()
);

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
