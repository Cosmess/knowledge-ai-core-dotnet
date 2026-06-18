using KnowledgeAi.Application.Common.Mediator;
using KnowledgeAi.Domain.Entities;

namespace KnowledgeAi.Application.Auth.Queries;

public sealed record GetCurrentUserQuery : IRequest<User>;
