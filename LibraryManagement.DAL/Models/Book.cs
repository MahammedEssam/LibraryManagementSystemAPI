
namespace LibraryManagement.DAL.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty; // For display when doing a JOIN
        public string ISBN { get; set; } = string.Empty;
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
    }
}
