using System;
using LibraryManagement.DAL;
using LibraryManagement.DAL.Exceptions;

namespace LibraryManagement.BLL
{
    public class LoanBLL
    {
        private readonly LoanDAL _loanDAL;

        public LoanBLL(LoanDAL loanDAL)
        {
            _loanDAL = loanDAL;
        }

        public int BorrowBook(int bookId, int memberId, int borrowDays = 14)
        {
            // Validations بسيطة
            if (bookId <= 0)
                throw new BusinessException("معرف الكتاب غير صحيح.");

            if (memberId <= 0)
                throw new BusinessException("معرف العضو غير صحيح.");

            if (borrowDays <= 0 || borrowDays > 60)
                throw new BusinessException("مدة الاستعارة يجب أن تكون بين 1 و 60 يوماً.");

            // استدعاء مباشر؛ الـ BusinessException والـ SqlException يستمران بالصعود إلى Controller
            return _loanDAL.BorrowBook(bookId, memberId, borrowDays);
        }

        public bool ReturnBook(int loanId)
        {
            if (loanId <= 0)
                throw new BusinessException("معرف الاستعارة غير صحيح.");

            return _loanDAL.ReturnBook(loanId);
        }
    }
}