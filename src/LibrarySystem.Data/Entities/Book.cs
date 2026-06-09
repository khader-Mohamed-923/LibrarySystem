using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibrarySystem.Data.Entities;

public class Book
{
   
    public int Id { get; set; }


    public string Title { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public string ISBN { get; set; } = string.Empty;

    public int TotalCopies { get; set; }


    public int AvailableCopies { get; set; }


    [Timestamp]
    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<Loan> Loans { get; set; } = [];
}
