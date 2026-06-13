using Moq;
using Shouldly;
using LibrarySystem.Contracts.Requests.Loan;
using LibrarySystem.Contracts.Responses.Loan;
using LibrarySystem.Services.Services.Implementations;
using LibrarySystem.Services.Exceptions;
using LibrarySystem.Data.Interfaces;
using LibrarySystem.Data.Entities;

namespace LibrarySystem.UnitTests.Services;


public class LoanServiceTests
{
   
    private readonly Mock<ILoanRepository>   _loanRepoMock;
    private readonly Mock<IBookRepository>   _bookRepoMock;
    private readonly Mock<IMemberRepository> _memberRepoMock;
    private readonly Mock<IUnitOfWork>        _unitOfWorkMock;
    private readonly FakeTimeProvider         _timeProvider;
    private readonly LoanService _sut; 

    public LoanServiceTests()
    {
        _loanRepoMock   = new Mock<ILoanRepository>();
        _bookRepoMock   = new Mock<IBookRepository>();
        _memberRepoMock = new Mock<IMemberRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _timeProvider   = new FakeTimeProvider(DateTimeOffset.UtcNow);

        _sut = new LoanService(
            _loanRepoMock.Object,
            _bookRepoMock.Object,
            _memberRepoMock.Object,
            _unitOfWorkMock.Object,
            _timeProvider
        );
    }


    [Fact]
    public async Task LoanBookAsync_WhenMemberHas3ActiveLoans_ThrowsBusinessRuleViolationException()
    {
        var member = ValidMember();
        var book   = AvailableBook();
        var request = new LoanBookRequest { MemberId = member.Id, BookId = book.Id };

        _memberRepoMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);
        _bookRepoMock  .Setup(r => r.GetByIdAsync(book.Id))  .ReturnsAsync(book);
        _loanRepoMock  .Setup(r => r.HasOverdueLoanAsync(member.Id)).ReturnsAsync(false);
        _loanRepoMock  .Setup(r => r.GetActiveLoanCountForMemberAsync(member.Id))
                       .ReturnsAsync(3);

        var ex = await Should.ThrowAsync<BusinessRuleViolationException>(
            () => _sut.LoanBookAsync(request)
        );

        ex.ErrorCode.ShouldBe(ErrorCode.LOAN_LIMIT_EXCEEDED);
    }

    [Fact]
    public async Task LoanBookAsync_WhenAvailableCopiesIsZero_ThrowsBusinessRuleViolationException()
    {
        var member = ValidMember();
        var book   = new Book { Id = 1, Title = "Clean Code", Author = "Robert Martin", ISBN = "9780132350884", AvailableCopies = 0, TotalCopies = 1 };
        var request = new LoanBookRequest { MemberId = member.Id, BookId = book.Id };

        _memberRepoMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);
        _bookRepoMock  .Setup(r => r.GetByIdAsync(book.Id))  .ReturnsAsync(book);

        var ex = await Should.ThrowAsync<BusinessRuleViolationException>(
            () => _sut.LoanBookAsync(request)
        );

        ex.ErrorCode.ShouldBe(ErrorCode.BOOK_NOT_AVAILABLE);
    }

    [Fact]
    public async Task LoanBookAsync_WhenMemberHasOutstandingFine_ThrowsBusinessRuleViolationException()
    {
        var member = new Member
        {
            Id              = 1,
            FirstName       = "Sara",
            LastName        = "Ahmed",
            Email           = "sara@test.com",
            Phone           = "1234567890",
            MembershipDate  = DateTime.UtcNow.AddYears(-1),
            OutstandingFine = 5.00m 
        };
        var book = AvailableBook();
        var request = new LoanBookRequest { MemberId = member.Id, BookId = book.Id };

        _memberRepoMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);
        _bookRepoMock  .Setup(r => r.GetByIdAsync(book.Id))  .ReturnsAsync(book);

        var ex = await Should.ThrowAsync<BusinessRuleViolationException>(
            () => _sut.LoanBookAsync(request)
        );

        ex.ErrorCode.ShouldBe(ErrorCode.OUTSTANDING_FINE);
    }

    [Fact]
    public async Task LoanBookAsync_WhenMemberHasOverdueBooks_ThrowsBusinessRuleViolationException()
    {
        var member = ValidMember();
        var book   = AvailableBook();
        var request = new LoanBookRequest { MemberId = member.Id, BookId = book.Id };

        _memberRepoMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);
        _bookRepoMock  .Setup(r => r.GetByIdAsync(book.Id))  .ReturnsAsync(book);
        _loanRepoMock  .Setup(r => r.HasOverdueLoanAsync(member.Id)).ReturnsAsync(true);

        var ex = await Should.ThrowAsync<BusinessRuleViolationException>(
            () => _sut.LoanBookAsync(request)
        );

        ex.ErrorCode.ShouldBe(ErrorCode.OVERDUE_BOOKS);
    }

    [Fact]
    public async Task LoanBookAsync_WhenAllRulesPassed_CallsLoanRepositoryAddAsyncExactlyOnce()
    {
        var member = ValidMember();
        var book   = AvailableBook();
        var request = new LoanBookRequest { MemberId = member.Id, BookId = book.Id };

        _memberRepoMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);
        _bookRepoMock  .Setup(r => r.GetByIdAsync(book.Id))  .ReturnsAsync(book);
        _loanRepoMock  .Setup(r => r.HasOverdueLoanAsync(member.Id)).ReturnsAsync(false);
        _loanRepoMock  .Setup(r => r.GetActiveLoanCountForMemberAsync(member.Id)).ReturnsAsync(0);

        await _sut.LoanBookAsync(request);

        _loanRepoMock.Verify(
            r => r.AddAsync(It.IsAny<Loan>()),
            Times.Once,
            "LoanRepository.AddAsync must be called exactly once on successful borrow"
        );
    }

    [Fact]
    public async Task ReturnBookAsync_WhenAlreadyReturned_ThrowsBusinessRuleViolationException()
    {
        var loan = new Loan
        {
            Id         = 5,
            MemberId   = 1,
            BookId     = 1,
            LoanDate   = DateTime.UtcNow.AddDays(-20),
            DueDate    = DateTime.UtcNow.AddDays(-6),
            IsReturned = true,
            ReturnedAt = DateTime.UtcNow.AddDays(-7),
            FineAmount = 0
        };

        _loanRepoMock.Setup(r => r.GetByIdAsync(loan.Id)).ReturnsAsync(loan);

        var ex = await Should.ThrowAsync<BusinessRuleViolationException>(
            () => _sut.ReturnBookAsync(loan.Id)
        );

        ex.ErrorCode.ShouldBe(ErrorCode.ALREADY_RETURNED);
    }

    [Fact]
    public async Task ReturnBookAsync_WhenLoanNotFound_ThrowsResourceNotFoundException()
    {
        _loanRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Loan?)null);

        var ex = await Should.ThrowAsync<ResourceNotFoundException>(
            () => _sut.ReturnBookAsync(999)
        );

        ex.ErrorCode.ShouldBe(ErrorCode.LOAN_NOT_FOUND);
    }

    [Fact]
    public async Task LoanBookAsync_WhenMemberNotFound_ThrowsResourceNotFoundException()
    {
        var request = new LoanBookRequest { MemberId = 999, BookId = 1 };

        _memberRepoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Member?)null);

        var ex = await Should.ThrowAsync<ResourceNotFoundException>(
            () => _sut.LoanBookAsync(request)
        );

        ex.ErrorCode.ShouldBe(ErrorCode.MEMBER_NOT_FOUND);
    }

    [Fact]
    public async Task LoanBookAsync_WhenBookNotFound_ThrowsResourceNotFoundException()
    {
        var member = ValidMember();
        var request = new LoanBookRequest { MemberId = member.Id, BookId = 999 };

        _memberRepoMock.Setup(r => r.GetByIdAsync(member.Id)).ReturnsAsync(member);
        _bookRepoMock  .Setup(r => r.GetByIdAsync(999)).ReturnsAsync((Book?)null);

        var ex = await Should.ThrowAsync<ResourceNotFoundException>(
            () => _sut.LoanBookAsync(request)
        );

        ex.ErrorCode.ShouldBe(ErrorCode.BOOK_NOT_FOUND);
    }


    private static Member ValidMember() => new()
    {
        Id             = 1,
        FirstName      = "Mohamed",
        LastName       = "Khaled",
        Email          = "mk@test.com",
        Phone          = "1234567890",
        MembershipDate = DateTime.UtcNow.AddYears(-1),
        OutstandingFine = 0
    };

    private static Book AvailableBook() => new()
    {
        Id              = 1,
        Title           = "Domain-Driven Design",
        Author          = "Eric Evans",
        ISBN            = "9780321125217",
        TotalCopies     = 3,
        AvailableCopies = 3 
    };
}


internal sealed class FakeTimeProvider : TimeProvider
{
    private DateTimeOffset _utcNow;

    public FakeTimeProvider(DateTimeOffset utcNow) => _utcNow = utcNow;

    public override DateTimeOffset GetUtcNow() => _utcNow;

    public void SetUtcNow(DateTimeOffset value) => _utcNow = value;
}
