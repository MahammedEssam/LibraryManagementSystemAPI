namespace LibraryManagement.DAL.Models
{
    public class Author
    {
        public int AuthorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Nationality { get; set; }
    }
}
