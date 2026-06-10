using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarySystem.Data.Entities;

public class Member
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public DateTime MembershipDate { get; set; }

    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<Loan> Loans { get; set; } = [];
}
