namespace LibrarySystem.Contracts.Responses.Loan;

public class LoanResponse
{
    public int Id { get; set; }
    public int MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int BookId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public DateTime LoanDate { get; set; }
    public DateTime DueDate { get; set; }
    public bool IsReturned { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public decimal FineAmount { get; set; }
}
