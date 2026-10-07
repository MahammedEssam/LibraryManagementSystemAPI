namespace LibraryManagement.DAL.Models
{
    public class Member
    {
        public int MemberId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public DateTime JoinDate { get; set; }
    }
}