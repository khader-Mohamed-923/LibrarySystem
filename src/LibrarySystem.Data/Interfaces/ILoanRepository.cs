using LibrarySystem.Data.Entities;

namespace LibrarySystem.Data.Interfaces;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(int id);
    Task AddAsync(Loan loan);
    void Update(Loan loan);
    Task<int> GetActiveLoanCountForMemberAsync(int memberId);
    Task<bool> HasOverdueLoanAsync(int memberId);
    Task<IEnumerable<Loan>> GetLoansByMemberAsync(int memberId);
}
