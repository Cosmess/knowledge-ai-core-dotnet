# Fluxo de Dados

## Fluxo De Ingestão

```txt
Source document
  -> normalize
  -> extract metadata
  -> split into chunks
  -> generate embeddings
  -> save document
  -> save chunks
  -> index vectors
```

## Fluxo De Chat

```txt
Authenticated user
  -> POST /chat
  -> validate authorization
  -> classify question
  -> retrieve chunks
  -> build prompt
  -> call LLM
  -> validate answer
  -> return answer + sources
```

## Fluxo MCP

```txt
Agent/IDE
  -> MCP tool call
  -> dev-knowledge-mcp
  -> POST /mcp/search
  -> retrieve structured context
  -> return sources and excerpts to agent
```

Os diagramas acima mantêm os nomes técnicos em inglês porque eles representam etapas internas do pipeline e contratos entre componentes.
