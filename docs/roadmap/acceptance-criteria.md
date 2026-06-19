# Critérios De Aceite

O projeto é considerado funcional quando:

- a API consegue ingerir Markdown;
- a API consegue ingerir páginas do Confluence;
- documentos são quebrados em chunks (por heading e por orçamento de palavras);
- conteúdo ingerido (Markdown e HTML do Confluence) é sanitizado antes do chunking, removendo tags/atributos potencialmente perigosos;
- headings são preservados como `headingPath` nos chunks tanto para Markdown quanto para Confluence;
- embeddings são armazenados no pgvector, com índice HNSW (`vector_cosine_ops`);
- reindexação incremental de páginas Confluence ocorre apenas quando a versão da página avança;
- `/chat` responde usando contexto recuperado;
- respostas citam fontes;
- `/chat` e `/mcp/search` aplicam autorização por `spaceKey`, respeitando a lista `AllowedSpaceKeys` do usuário;
- o MCP consegue buscar contexto técnico, propagando a identidade do usuário (JWT) além da API Key de serviço, para que a autorização por `spaceKey` seja aplicada também a chamadas via MCP;
- respostas MCP incluem fontes e status de evidência;
- a API recusa responder quando a evidência é insuficiente;
- a API expõe contrato OpenAPI/Swagger real (Swashbuckle, disponível em `/swagger` no ambiente de Development);
- Api e Mcp possuem Dockerfiles próprios, integrados ao `docker-compose`;
- o projeto roda com Docker Compose;
- dados sensíveis não são expostos em logs ou respostas (mascaramento via Serilog já ativo).

## Recém-resolvidos

Os itens abaixo já estavam pendentes em versões anteriores deste documento e foram resolvidos nesta rodada de correções:

- autorização por `spaceKey` aplicada de fato em `/chat` e em `/mcp/search`;
- identidade de usuário real propagada pelo MCP via JWT (`KNOWLEDGE_USER_JWT`), em vez de uma única identidade de serviço compartilhada;
- sanitização básica de conteúdo ingerido (Markdown e HTML do Confluence);
- preservação de headings no chunking de páginas Confluence (antes, headings eram silenciosamente perdidos para essa fonte).

Também já estavam resolvidos antes desta rodada, mas continuavam descritos como pendentes em versões antigas deste documento:

- testes reais do MCP via cliente JSON-RPC sobre stdio (initialize / tools-list / tools-call);
- reindexação incremental de páginas Confluence baseada em versão;
- containerização de Api e Mcp com Dockerfiles próprios, integrados ao `docker-compose`.

## Fora Do Escopo Deste Repositório

Critérios abaixo dependem do `knowledge-ai-web` (frontend, em outro repositório) e não se aplicam a este repositório, que é exclusivamente backend:

- frontend autenticado;
- chat web;
- histórico visual;
- feedback pela interface.
