using LibrarySystem.Contracts.Requests.Borrowing;
using LibrarySystem.Contracts.Responses.Borrowing;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Interfaces;
using LibrarySystem.Services.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Services.Implementations;

public class BorrowingService
{
    private const int MaxActiveBorrowings = 5;
    private const int LoanPeriodDays = 14;

    private readonly IBorrowingRepository _borrowingRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IMemberRepository _memberRepository;
    private readonly IUnitOfWork _unitOfWork;

    public BorrowingService(
        IBorrowingRepository borrowingRepository,
        IBookRepository bookRepository,
        IMemberRepository memberRepository,
        IUnitOfWork unitOfWork)
    {
        _borrowingRepository = borrowingRepository;
        _bookRepository = bookRepository;
        _memberRepository = memberRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BorrowingResponse> BorrowBookAsync(BorrowBookRequest request)
    {
        var member = await _memberRepository.GetByIdAsync(request.MemberId);
        if (member is null)
            throw new ResourceNotFoundException(ErrorCode.MEMBER_NOT_FOUND);

        var book = await _bookRepository.GetByIdAsync(request.BookId);
        if (book is null)
            throw new ResourceNotFoundException(ErrorCode.BOOK_NOT_FOUND);

        if (book.AvailableCopies <= 0)
            throw new BookNotAvailableException();

        if (member.OutstandingFine > 0)
            throw new MemberHasOutstandingFinesException();

        var hasOverdue = await _borrowingRepository.HasOverdueBorrowingAsync(request.MemberId);
        if (hasOverdue)
            throw new MemberHasOverdueBooksException();

        var activeCount = await _borrowingRepository.GetActiveBorrowingCountForMemberAsync(request.MemberId);
        if (activeCount >= MaxActiveBorrowings)
            throw new MaxBorrowingLimitReachedException();

        var utcNow = DateTime.UtcNow;
        var loan = new Loan
        {
            MemberId = request.MemberId,
            BookId = request.BookId,
            BorrowDate = utcNow,
            DueDate = utcNow.AddDays(LoanPeriodDays),
            IsReturned = false
        };

        const int maxRetries = 3;
        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            // 👈 بنستخدم الـ bookRepository الجاهز اللي موجود عندك في السيرفيس فعلاً!
            var trackedBook = await _bookRepository.GetByIdAsync(request.BookId);

            if (trackedBook is null || trackedBook.AvailableCopies <= 0)
                throw new BookNotAvailableException();

            trackedBook.AvailableCopies--;

            try
            {
                if (attempt == 1)
                {
                    await _borrowingRepository.AddAsync(loan);
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

                // 👈 بنلقط الـ Entry ونعمل Reload من الـ EF Core مباشرة بدون أي Repositories زيادة
                var entry = ex.Entries.Single(e => e.Entity is Book);
                await entry.ReloadAsync();
            }
        }

        throw new ConcurrencyException(ErrorCode.CONCURRENCY_CONFLICT);
    }

    private static BorrowingResponse MapToResponse(Loan loan)
    {
        return new BorrowingResponse
        {
            Id = loan.Id,
            MemberId = loan.MemberId,
            MemberName = $"{loan.Member.FirstName} {loan.Member.LastName}",
            BookId = loan.BookId,
            BookTitle = loan.Book.Title,
            BorrowDate = loan.BorrowDate,
            DueDate = loan.DueDate,
            IsReturned = loan.IsReturned
        };
    }
}