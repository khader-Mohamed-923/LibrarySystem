using LibrarySystem.Data.Entities;

namespace LibrarySystem.Data.Interfaces;

public interface IMemberRepository
{
    Task<Member?> GetByIdAsync(int id);
    Task<IReadOnlyList<Member>> GetAllAsync();
    Task AddAsync(Member member);
    void Update(Member member, byte[]? rowVersion = null);
    void Delete(Member member);
    Task<bool> IsEmailUniqueAsync(string email, int? excludeId = null);
}
