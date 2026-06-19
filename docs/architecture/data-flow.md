# Fluxo de Dados

## Fluxo De Ingestão — Markdown

```txt
Arquivos Markdown
  -> IMarkdownLoader (Markdig) carrega o conteúdo
  -> sanitização do conteúdo (remove <script>/<style>/<iframe>/<object>/<embed>/<noscript>
     e atributos on*=/javascript:)
  -> HeadingChunkingService: split por heading Markdown (#), merge de seções pequenas,
     split por orçamento de 450 palavras (não tokens)
  -> geração de embedding por chunk
  -> salva documento e chunks no PostgreSQL
```

## Fluxo De Ingestão — Confluence

```txt
Páginas Confluence (IConfluenceClient, formato storage/HTML)
  -> sanitização do HTML (mesma lista de tags/atributos perigosos do Markdown)
  -> normalização HTML -> texto (HtmlAgilityPackNormalizer), convertendo <h1>-<h6>
     em prefixos Markdown (# .. ######) antes de extrair o texto, para que o
     chunking preserve o headingPath também para páginas Confluence
  -> HeadingChunkingService (mesmo chunking usado para Markdown)
  -> geração de embedding por chunk
  -> salva documento e chunks no PostgreSQL
```

Reindexação incremental: uma página Confluence só é reprocessada se a versão dela avançou desde a última ingestão (comparação com `Document.Version` salvo).

Não existe uma etapa separada de "indexação" pós-save: o índice vetorial é um índice HNSW (`vector_cosine_ops`) já criado no schema, e a inserção do embedding no `document_chunks.embedding` é direta.

Fontes de ingestão implementadas: apenas Markdown e Confluence (`DocumentSource` só tem esses dois valores). Outras categorias de metadados (ex.: OpenAPI/Swagger, runbooks, backlog) não têm pipeline de ingestão próprio — só existem como documentos classificados manualmente com esses metadados.

## Fluxo De Chat

```txt
Usuário autenticado
  -> POST /chat
  -> valida autorização (inclui checagem de spaceKey contra AllowedSpaceKeys do usuário)
  -> AskQuestionCommandHandler: retrieval de chunks relevantes
  -> monta prompt
  -> chama o provedor de LLM configurado (OpenAI, Anthropic ou Ollama)
  -> valida evidência/confiança da resposta
  -> retorna resposta + fontes
```

## Fluxo MCP

```txt
Agente/IDE
  -> chamada de tool MCP (stdio, JSON-RPC)
  -> KnowledgeAi.Mcp (KnowledgeApiClient)
  -> POST /mcp/search, com API Key + JWT de usuário
  -> Knowledge API aplica autorização por spaceKey e faz o retrieval
  -> retorna contexto estruturado (fontes e trechos) ao agente
```

Os diagramas acima mantêm os nomes técnicos em inglês porque eles representam etapas internas do pipeline e contratos entre componentes.
