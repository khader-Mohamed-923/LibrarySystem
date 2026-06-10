namespace LibrarySystem.Contracts.Requests.Member;

public class UpdateMemberDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public byte[]? RowVersion { get; set; }
}
