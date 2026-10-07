using System;
using System.Data;
using Microsoft.Data.SqlClient;
using LibraryManagement.DAL.Exceptions;
using LibraryManagement.DAL;

namespace LibraryManagement.DAL
{
    public class LoanDAL
    {
        private readonly DatabaseConnection _db;

        public LoanDAL(DatabaseConnection db)
        {
            _db = db;
        }

        public int BorrowBook(int bookId, int memberId, int borrowDays)
        {
            using (SqlConnection conn = new SqlConnection(_db.CounnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_BorrowBook", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    cmd.Parameters.AddWithValue("@MemberId", memberId);
                    cmd.Parameters.AddWithValue("@BorrowDays", borrowDays);

                    try
                    {
                        conn.Open();
                        object result = cmd.ExecuteScalar();

                        if (result != null && int.TryParse(result.ToString(), out int newLoanId))
                        {
                            return newLoanId;
                        }

                        throw new Exception("فشل الحصول على رقم الاستعارة الجديد.");
                    }
                    catch (SqlException ex)
                    {
                        // ex.Number == 50000 تعني أن الخطأ تم رفعه عبر RAISERROR مخصص في SQL Server
                        if (ex.Number == 50000)
                        {
                            throw new BusinessException(ex.Message);
                        }

                        // خطأ تقني حقيقي (مثل انقطاع الاتصال أو السيرفر دوان) -> اتركه يصعد كـ SqlException
                        throw;
                    }
                }
            }
        }

        public bool ReturnBook(int loanId)
        {
            using (SqlConnection conn = new SqlConnection(_db.CounnectionString))
            {
                using (SqlCommand cmd = new SqlCommand("sp_ReturnBook", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@LoanId", loanId);

                    try
                    {
                        conn.Open();
                        cmd.ExecuteNonQuery();
                        return true;
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 50000)
                        {
                            throw new BusinessException(ex.Message);
                        }

                        throw;
                    }
                }
            }
        }

        // فحص هل العضو لديه استعارات نشطة
        public bool HasActiveLoansByMemberId(int memberId)
        {
            using (SqlConnection conn = new SqlConnection(_db.CounnectionString))
            {
                string query = "SELECT COUNT(1) FROM Loans WHERE MemberId = @MemberId AND ReturnDate IS NULL";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MemberId", memberId);
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        // فحص هل الكتاب مستعار حالياً (له استعارات نشطة)
        public bool HasActiveLoansByBookId(int bookId)
        {
            using (SqlConnection conn = new SqlConnection(_db.CounnectionString))
            {
                string query = "SELECT COUNT(1) FROM Loans WHERE BookId = @BookId AND ReturnDate IS NULL";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@BookId", bookId);
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }
    }
}