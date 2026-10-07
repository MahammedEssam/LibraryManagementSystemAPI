using System;

namespace LibraryManagement.DAL.Models
{
    public enum LoanStatus : byte
    {
        Active = 1,
        Returned = 2,
        Overdue = 3
    }

    public class Loan
    {
        public int LoanId { get; set; }
        public int BookId { get; set; }
        public int MemberId { get; set; }
        public DateTime LoanDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public LoanStatus Status { get; set; }

        // Navigation Properties (اختياري للـ Display/JOINs)
        public string BookTitle { get; set; }
        public string MemberName { get; set; }
    }
}