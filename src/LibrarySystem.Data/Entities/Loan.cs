namespace LibrarySystem.Data.Entities;


public class Loan
{
 
    public int Id { get; set; }


    public int BookId { get; set; }


    public  Book Book { get; set; } = null!;
}
