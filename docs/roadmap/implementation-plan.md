# Plano De Implementação

## Fase 1: Scaffold Core

- Criar monorepo.
- Criar app da API.
- Criar app MCP.
- Criar packages compartilhados.
- Criar documentação SDD.

Status: concluída.

## Fase 2: Backend API

- Adicionar autenticação.
- Adicionar OpenAPI.
- Adicionar endpoint de chat.
- Adicionar endpoint de busca MCP.
- Adicionar endpoints de documentos.

Status: concluída para MVP.

## Fase 3: Markdown RAG

- Carregar Markdown.
- Fazer parse de frontmatter.
- Gerar chunks.
- Gerar embeddings.
- Armazenar chunks em pgvector.
- Recuperar contexto.

Status: concluída para MVP com embeddings locais determinísticos e OpenAI opcional.

## Fase 4: MCP

- Adicionar tools iniciais.
- Integrar com a API.
- Retornar fontes estruturadas.
- Testar com clientes IDE/agentes.

Status: tools e configuração criadas; testes reais com clientes ainda pendentes.

## Fase 5: Confluence

- Buscar páginas.
- Normalizar HTML.
- Controlar versões.
- Reindexar páginas alteradas.

Status: ingestão básica concluída; reindexação incremental fina ainda pendente.

## Fase 6: Segurança E LLMOps

- Adicionar autorização.
- Adicionar logs e métricas.
- Mascarar dados sensíveis.
- Adicionar fallback de providers.
- Adicionar avaliação de respostas.

Status: métricas e testes iniciais concluídos; avaliação avançada ainda pendente.
