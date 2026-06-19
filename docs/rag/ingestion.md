# Ingestão RAG

Fontes implementadas:

- arquivos Markdown
- páginas do Confluence

Não há pipeline de ingestão próprio para OpenAPI/Swagger, documentação em repositórios, runbooks ou backlog/histórias de usuário — esses só existem como valores de metadado (`DocumentType`/`KnowledgeDomain`) que podem ser atribuídos a documentos ingeridos via Markdown ou Confluence.

## Fluxo Markdown (`IngestMarkdownCommandHandler`)

```txt
read files (IMarkdownLoader, Markdig)
  -> parse frontmatter
  -> sanitize content (remove <script>/<style>/<iframe>/<object>/<embed>/<noscript> e atributos on*=/javascript:)
  -> chunk (split by heading, merge small sections, split by word budget)
  -> embed each chunk
  -> upsert document record
  -> save chunks (delete + insert transacional)
```

## Fluxo Confluence (`IngestConfluenceCommandHandler`)

```txt
fetch pages by space (IConfluenceClient)
  -> capture title, body (HTML), version, url, updatedAt
  -> sanitize HTML (mesma lista de tags/atributos perigosos do fluxo Markdown)
  -> normalize HTML to text (converte <h1>-<h6> em headings estilo Markdown antes de extrair o texto)
  -> chunk
  -> embed each chunk
  -> upsert document record
  -> save chunks
```

Reindexação incremental: uma página só é reprocessada se a versão dela no Confluence avançou desde a última ingestão (comparação com `Document.Version` salvo) — evita reingestão completa a cada chamada de `/reindex`.
