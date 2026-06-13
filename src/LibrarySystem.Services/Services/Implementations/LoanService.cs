using LibrarySystem.Contracts.Requests.Loan;
using LibrarySystem.Contracts.Responses.Loan;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Interfaces;
using LibrarySystem.Services.Exceptions;
using LibrarySystem.Services.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Services.Services.Implementations;

public class LoanService : ILoanService
{
    private const int MaxActiveLoans = 3;
    private const int LoanPeriodDays = 14;
    private const decimal FinePerDay = 0.50m;

    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider; 

    public LoanService(
        ILoanRepository loanRepository,
        IBookRepository bookRepository,
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider) 
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
        _timeProvider = timeProvider;
    }

    public async Task<LoanResponse> LoanBookAsync(LoanBookRequest request)
    {
        // 1. Transaction Management: Wrap everything to ensure atomic Read -> Lock -> Update -> Commit
        await using var transaction = await _unitOfWork.BeginTransactionAsync();

        try
        {
            var member = await _memberRepository.GetByIdAsync(request.MemberId);
            if (member is null)
                throw new ResourceNotFoundException(ErrorCode.MEMBER_NOT_FOUND);

            // 2. Pessimistic Locking: Fetch with UPDLOCK to lock the row in the database until transaction ends
            var trackedBook = await _bookRepository.GetByIdWithUpdLockAsync(request.BookId);
            
            if (trackedBook is null)
                throw new ResourceNotFoundException(ErrorCode.BOOK_NOT_FOUND);

            if (trackedBook.AvailableCopies <= 0)
                throw new BusinessRuleViolationException(ErrorCode.BOOK_NOT_AVAILABLE);

            if (member.OutstandingFine > 0)
                throw new BusinessRuleViolationException(ErrorCode.OUTSTANDING_FINE);

            var hasOverdue = await _loanRepository.HasOverdueLoanAsync(request.MemberId);
            if (hasOverdue)
                throw new BusinessRuleViolationException(ErrorCode.OVERDUE_BOOKS);

            var activeCount = await _loanRepository.GetActiveLoanCountForMemberAsync(request.MemberId);
            if (activeCount >= MaxActiveLoans)
                throw new BusinessRuleViolationException(ErrorCode.LOAN_LIMIT_EXCEEDED);

            var utcNow = _timeProvider.GetUtcNow().DateTime;
            
            var loan = new Loan
            {
                MemberId = request.MemberId,
                BookId = request.BookId,
                LoanDate = utcNow,
                DueDate = utcNow.AddDays(LoanPeriodDays),
                IsReturned = false
            };

            await _loanRepository.AddAsync(loan);

            trackedBook.AvailableCopies--;

            await _unitOfWork.SaveChangesAsync();
            
            await transaction.CommitAsync();

            loan.Book = trackedBook;
            loan.Member = member;

            return MapToResponse(loan);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            
            // 3. Concurrency Handling: Map EF concurrency conflicts to HTTP 409
            throw new ConcurrencyException(ErrorCode.CONCURRENCY_CONFLICT);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<LoanResponse> ReturnBookAsync(int loanId)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId);
        if (loan is null)
            throw new ResourceNotFoundException(ErrorCode.LOAN_NOT_FOUND);

        if (loan.IsReturned)
            throw new BusinessRuleViolationException(ErrorCode.ALREADY_RETURNED);

      
        var utcNow = _timeProvider.GetUtcNow().DateTime;
        
        const int maxRetries = 3;
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            loan.IsReturned = true;
            loan.ReturnedAt = utcNow;

            
            if (utcNow > loan.DueDate)
            {
                var overdueDays = (int)(utcNow - loan.DueDate).TotalDays;
                if (overdueDays > 0)
                {
                    loan.FineAmount = overdueDays * FinePerDay;
                    loan.Member.OutstandingFine += loan.FineAmount;
                }
            }

            if (loan.Book != null)
            {
                loan.Book.AvailableCopies++;
            }

            try
            {
                await _unitOfWork.SaveChangesAsync();
                return MapToResponse(loan);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (attempt == maxRetries)
                    throw new ConcurrencyException(ErrorCode.CONCURRENCY_CONFLICT);

                
                foreach (var entry in ex.Entries)
                {
                    await entry.ReloadAsync();
                }
            }
        }

        throw new ConcurrencyException(ErrorCode.CONCURRENCY_CONFLICT);
    }

    public async Task<IEnumerable<LoanResponse>> GetLoansByMemberAsync(int memberId)
    {
        var member = await _memberRepository.GetByIdAsync(memberId);
        if (member is null)
            throw new ResourceNotFoundException(ErrorCode.MEMBER_NOT_FOUND);

        var loans = await _loanRepository.GetLoansByMemberAsync(memberId);
        return loans.Select(MapToResponse);
    }

    private static LoanResponse MapToResponse(Loan loan)
    {
        return new LoanResponse
        {
            Id = loan.Id,
            MemberId = loan.MemberId,
            MemberName = $"{loan.Member.FirstName} {loan.Member.LastName}",
            BookId = loan.BookId,
            BookTitle = loan.Book.Title,
            LoanDate = loan.LoanDate,
            DueDate = loan.DueDate,
            IsReturned = loan.IsReturned,
            ReturnedAt = loan.ReturnedAt,
            FineAmount = loan.FineAmount
        };
    }
}