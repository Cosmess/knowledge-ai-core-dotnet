# Estratégia De Prompts

O system prompt real (`AskQuestionCommandHandler.BuildSystemPrompt`) está escrito **em português**, com duas variantes por `Audience`. O prompt do usuário é montado como:

```txt
Contexto:
{conteúdo dos chunks recuperados, concatenados com linha em branco entre eles}

Pergunta: {pergunta do usuário}
```

O texto do contexto traz só o `Content` de cada chunk — não inclui título/URL da fonte explicitamente; a citação de fontes (regra abaixo) depende do próprio conteúdo do chunk, já que a referência estruturada (`sources[]`) é devolvida separadamente na resposta da API.

## Prompt Para Desenvolvedores (`Audience.Developers`)

> Use apenas o contexto recuperado, cite fontes, separe regras de negócio de detalhes técnicos, mencione quando faltar evidência, nunca exponha segredos, identifique conflitos entre documentos.

## Prompt Para Demais Audiências (`Operations`/`Product`/`Support`/`Viewer`)

> Use apenas o contexto recuperado, explique em linguagem simples, priorize processo e regra de negócio, evite detalhes internos de infraestrutura, recomende validação com o time responsável quando houver risco operacional.

## Fallback Extrativo

Se o provedor de LLM falhar, a resposta cai para o conteúdo do chunk mais relevante (maior score), em vez de propagar o erro ao usuário.

## Versionamento

Ainda não implementado: o prompt é fixo no código (sem versionamento em runtime). A tabela `prompt_versions` existe no schema do banco, mas nenhum código a usa hoje — versionamento de prompt é roadmap (ver `docs/operations/llmops.md`).
