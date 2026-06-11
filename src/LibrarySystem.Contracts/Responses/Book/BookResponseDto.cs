namespace LibrarySystem.Contracts.Responses.Book;

public class BookResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Isbn { get; set; } = string.Empty;
    public int TotalCopies { get; set; }
    public int AvailableCopies { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
