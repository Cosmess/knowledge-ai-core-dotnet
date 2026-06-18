# Chunking

O chunking deve preservar contexto suficiente para que um trecho recuperado seja útil.

Regras:

- preservar headings
- preservar metadados do documento
- manter referência à fonte
- evitar chunks grandes demais
- separar por tipo de documento quando fizer sentido
- incluir títulos de seções pai

Estratégia inicial:

```txt
split by heading
  -> merge small sections
  -> split large sections by token budget
  -> attach metadata
```
