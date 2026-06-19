# Taxonomia De Metadados

## Onde os metadados vivem

Os campos `source`, `spaceKey`, `documentType`, `audience`, `system`, `title`, `url`, `version` e `updatedAt` são colunas relacionais da entidade `Document` (tabela `documents`) — não um blob JSON.

O único metadado em formato JSON real é `DocumentChunk.Metadata` (coluna `document_chunks.metadata`, tipo `jsonb`), que na prática contém apenas a chave `headingPath`: o caminho hierárquico do heading do chunk, por exemplo `"Título > Subtítulo"`.

## Domínios (`KnowledgeDomain`)

Enum em `KnowledgeAi.Domain.ValueObjects.KnowledgeDomain`:

```txt
Technical
Business
Operations
Product
Onboarding
Architecture
Api
Incident
Backlog
Runbook
Troubleshooting
```

## Tipos De Documento (`DocumentType`)

Enum em `KnowledgeAi.Domain.ValueObjects.DocumentType`:

```txt
TechnicalDoc
BusinessRule
ApiDoc
ArchitectureDecision
UserStory
NextTask
Runbook
Faq
OnboardingDoc
ProductDoc
OperationalProcess
EventContract
DatabaseDoc
IntegrationDoc
```

Nem todo `DocumentType`/`KnowledgeDomain` tem um pipeline de ingestão dedicado: apenas Markdown e Confluence são fontes de ingestão implementadas (`DocumentSource`). Documentos com outras combinações de tipo/domínio (ex.: `ApiDoc`, `Runbook`, `NextTask`) só existem se forem classificados manualmente com esses metadados ao ingerir via Markdown ou Confluence.

## Audiência (`Audience`)

Enum em `KnowledgeAi.Domain.ValueObjects.Audience` — um tipo restrito do C#, não uma string livre:

```txt
Developers
Operations
Product
Support
Viewer
```

## Exemplo De Documento

Os valores abaixo ilustram os campos da entidade `Document`, todos colunas relacionais (não um JSON):

```txt
source:       Confluence
spaceKey:     ENG
documentType: BusinessRule
audience:     Developers
system:       settlement-service
title:        Regras de Liquidacao
url:          https://empresa.atlassian.net/wiki/...
version:      12
updatedAt:    2026-06-02T10:00:00Z
```

O `DocumentChunk` correspondente carrega, além do `content` e do `embedding`, o `domain` (`KnowledgeDomain`) e o `metadata` jsonb com o `headingPath`, por exemplo:

```json
{
  "headingPath": "Regras de Liquidacao > Janela de Processamento"
}
```
