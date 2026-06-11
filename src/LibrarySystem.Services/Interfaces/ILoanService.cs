using LibrarySystem.Contracts.Requests.Loan;
using LibrarySystem.Contracts.Responses.Loan;

namespace LibrarySystem.Services.Interfaces;

public interface ILoanService
{
    Task<LoanResponse> LoanBookAsync(LoanBookRequest request);
    Task<LoanResponse> ReturnBookAsync(int loanId);
    Task<IEnumerable<LoanResponse>> GetLoansByMemberAsync(int memberId);
}
