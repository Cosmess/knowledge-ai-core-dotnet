# Chunking

O chunking preserva contexto suficiente para que um trecho recuperado seja útil.

Regras:

- preservar headings
- preservar referência à fonte (via `Document`, ligado a cada `DocumentChunk`)
- evitar chunks grandes demais
- manter títulos de seções pai (heading path)

## Estratégia Real (`HeadingChunkingService`)

```txt
split by Markdown heading (#, ##, ... ######)
  -> track nested heading path ("Título > Subtítulo")
  -> merge sections smaller than 30 words into the previous section
  -> split sections larger than 450 words into multiple chunks
  -> attach heading path as chunk metadata
```

Constantes reais: `WordBudget = 450` (orçamento de **palavras**, não tokens — não há tokenizer no pipeline), `MinWordsPerSection = 30`.

A detecção de heading é feita por regex linha a linha (`^(#{1,6})\s+`), reconhecendo apenas headings estilo Markdown.

## Confluence (HTML)

O conteúdo Confluence é HTML, não Markdown. Antes do chunking, a normalização (`HtmlAgilityPackNormalizer`) converte cada `<h1>`-`<h6>` em um prefixo Markdown (`#` a `######`, conforme o nível) e só então extrai o texto — isso garante que o `HeadingChunkingService` (que só reconhece `#` estilo Markdown) também preserve `headingPath` para páginas Confluence.
