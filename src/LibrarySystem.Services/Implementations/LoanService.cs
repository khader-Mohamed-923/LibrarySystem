using LibrarySystem.Contracts.Requests.Loan;
using LibrarySystem.Contracts.Responses.Loan;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Interfaces;
using LibrarySystem.Services.Exceptions;
using LibrarySystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Services.Implementations;

public class LoanService : ILoanService
{
    private const int MaxActiveLoans = 5;
    private const int LoanPeriodDays = 14;
    private const decimal FinePerDay = 0.50m;

    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LoanService(
        ILoanRepository loanRepository,
        IBookRepository bookRepository,
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork)
    {
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<LoanResponse> LoanBookAsync(LoanBookRequest request)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId);
        if (member is null)
            throw new ResourceNotFoundException(ErrorCode.MEMBER_NOT_FOUND);

        var book = await _bookRepository.GetByIdAsync(request.BookId);
        if (book is null)
            throw new ResourceNotFoundException(ErrorCode.BOOK_NOT_FOUND);

        if (book.AvailableCopies <= 0)
            throw new BusinessRuleViolationException(ErrorCode.BOOK_NOT_AVAILABLE);

        if (member.OutstandingFine > 0)
            throw new BusinessRuleViolationException(ErrorCode.OUTSTANDING_FINE);

        var hasOverdue = await _loanRepository.HasOverdueLoanAsync(request.MemberId);
        if (hasOverdue)
            throw new BusinessRuleViolationException(ErrorCode.OVERDUE_BOOKS);

        var activeCount = await _loanRepository.GetActiveLoanCountForMemberAsync(request.MemberId);
        if (activeCount >= MaxActiveLoans)
            throw new BusinessRuleViolationException(ErrorCode.LOAN_LIMIT_EXCEEDED);

        var utcNow = DateTime.UtcNow;
        var loan = new Loan
        {
            MemberId = request.MemberId,
            BookId = request.BookId,
            LoanDate = utcNow,
            DueDate = utcNow.AddDays(LoanPeriodDays),
            IsReturned = false
        };

        const int maxRetries = 3;
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            var trackedBook = await _bookRepository.GetByIdAsync(request.BookId);

            if (trackedBook is null || trackedBook.AvailableCopies <= 0)
                throw new BusinessRuleViolationException(ErrorCode.BOOK_NOT_AVAILABLE);

            trackedBook.AvailableCopies--;

            try
            {
                if (attempt == 1)
                {
                    await _loanRepository.AddAsync(loan);
                }

                await _unitOfWork.SaveChangesAsync();

                loan.Book = trackedBook;
                loan.Member = member;

                return MapToResponse(loan);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (attempt == maxRetries)
                    throw new ConcurrencyException(ErrorCode.CONCURRENCY_CONFLICT);

                var entry = ex.Entries.Single(e => e.Entity is Book);
                await entry.ReloadAsync();
            }
        }

        throw new ConcurrencyException(ErrorCode.CONCURRENCY_CONFLICT);
    }

    public async Task<LoanResponse> ReturnBookAsync(int loanId)
    {
        var loan = await _loanRepository.GetByIdAsync(loanId);
        if (loan is null)
            throw new ResourceNotFoundException(ErrorCode.LOAN_NOT_FOUND);

        if (loan.IsReturned)
            throw new BusinessRuleViolationException(ErrorCode.ALREADY_RETURNED);

        var utcNow = DateTime.UtcNow;
        loan.IsReturned = true;
        loan.ReturnedAt = utcNow;

        // Calculate fine if overdue
        if (utcNow > loan.DueDate)
        {
            var overdueDays = (int)(utcNow - loan.DueDate).TotalDays;
            if (overdueDays > 0)
            {
                loan.FineAmount = overdueDays * FinePerDay;
                loan.Member.OutstandingFine += loan.FineAmount;
            }
        }

        _loanRepository.Update(loan);
        _memberRepository.Update(loan.Member);

        // Increment available copies
        var book = await _bookRepository.GetByIdAsync(loan.BookId);
        if (book != null)
        {
            book.AvailableCopies++;
            _bookRepository.Update(book);
        }

        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(loan);
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