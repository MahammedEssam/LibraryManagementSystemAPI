namespace LibraryManagement.API.DTOs
{
    public class BorrowBookRequestDto
    {
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public int BorrowDays { get; set; } = 14;
    }
}