using KnowledgeAi.Domain.Entities;

namespace KnowledgeAi.Application.Common.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken);

    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Inserts the user unless a user with the same email already exists. Returns true if it was created.</summary>
    Task<bool> CreateIfNotExistsAsync(User user, CancellationToken cancellationToken);
}
