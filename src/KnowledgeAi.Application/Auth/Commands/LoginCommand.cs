using KnowledgeAi.Application.Common.Interfaces;
using KnowledgeAi.Application.Common.Mediator;

namespace KnowledgeAi.Application.Auth.Commands;

public sealed record LoginCommand(string Email, string Password) : IRequest<AccessToken>;
