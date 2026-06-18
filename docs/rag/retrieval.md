# Retrieval

O retrieval deve suportar:

- busca semântica
- busca por palavra-chave
- busca híbrida
- filtros por domínio
- filtros por audiência
- filtros por sistema
- filtros por space
- filtros por tipo de documento
- filtros por data de atualização e versão

MVP:

```txt
PostgreSQL + pgvector similarity search
```

Depois:

```txt
hybrid search with text ranking + vector score
reranking
semantic cache
```
