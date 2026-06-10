namespace LibrarySystem.Contracts.Responses.Member;

public class MemberResponseDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime MembershipDate { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
