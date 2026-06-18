# Ingestão RAG

Fontes:

- arquivos Markdown
- páginas do Confluence
- documentos OpenAPI/Swagger
- documentação em repositórios
- runbooks
- backlog e histórias de usuário

## Fluxo Markdown

```txt
read files
  -> parse frontmatter
  -> preserve headings
  -> normalize content
  -> create document record
  -> chunk
  -> embed
  -> save chunks
```

## Fluxo Confluence

```txt
fetch pages by space
  -> capture title, body, version, url, updatedAt
  -> normalize HTML
  -> create document record
  -> chunk
  -> embed
  -> save chunks
```
