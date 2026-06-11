namespace LibrarySystem.Contracts.Requests.Book;

public class UpdateBookDto
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Isbn { get; set; }
    public byte[]? RowVersion { get; set; }
}
