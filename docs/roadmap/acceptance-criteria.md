# Critérios De Aceite

O projeto é considerado funcional quando:

- a API consegue ingerir Markdown;
- a API consegue ingerir páginas do Confluence;
- documentos são quebrados em chunks;
- embeddings são armazenados no pgvector;
- `/chat` responde usando contexto recuperado;
- respostas citam fontes;
- o MCP consegue buscar contexto técnico;
- respostas MCP incluem fontes e status de evidência;
- a API recusa responder quando a evidência é insuficiente;
- o frontend consegue consumir contratos OpenAPI;
- o projeto roda com Docker Compose;
- dados sensíveis não são expostos em logs ou respostas.

Critérios ainda dependentes do `knowledge-ai-web`:

- frontend autenticado;
- chat web;
- histórico visual;
- feedback pela interface.
