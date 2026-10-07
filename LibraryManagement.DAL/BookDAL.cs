using Microsoft.Data.SqlClient;
using System.Data;
using LibraryManagement.DAL.Models;
using LibraryManagement.DAL.Exceptions;

namespace LibraryManagement.DAL
{
    public class BookDAL
    {
        private readonly DatabaseConnection _db;
        public BookDAL(DatabaseConnection db)
        {
            _db = db;
        }

        public List<Book> GetAllBooks()
        {
            var Books = new List<Book>();

            using (var conn = new SqlConnection(_db.CounnectionString))
            using (var cmd = new SqlCommand("sp_GetAllBooks", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    while(reader.Read())
                    {
                        Books.Add(new Book
                        {
                            BookId = Convert.ToInt32(reader["BookId"]),
                            Title = reader["Title"].ToString()!,
                            AuthorId = Convert.ToInt32(reader["AuthorId"]),
                            AuthorName = reader["AuthorName"].ToString()!,
                            ISBN = reader["ISBN"].ToString()!,
                            TotalCopies = Convert.ToInt32(reader["TotalCopies"]),
                            AvailableCopies = Convert.ToInt32(reader["AvailableCopies"])
                        });
                    }
                }
            }
            return Books;
        }

        public Book? GetBookById (int bookId)
        {
            using (var conn = new SqlConnection(_db.CounnectionString))
            using (var cmd = new SqlCommand("sp_GetBookById", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BookId", bookId);
                conn.Open();

                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Book
                        {
                            BookId = Convert.ToInt32(reader["BookId"]),
                            Title = reader["Title"].ToString()!,
                            AuthorId = Convert.ToInt32(reader["AuthorId"]),
                            AuthorName = reader["AuthorName"].ToString()!,
                            ISBN = reader["ISBN"].ToString()!,
                            TotalCopies = Convert.ToInt32(reader["TotalCopies"]),
                            AvailableCopies = Convert.ToInt32(reader["AvailableCopies"])
                        };
                    }
                }
            }
            return null;
        }

        public int AddBook(Book book)
        {
            using(var conn = new SqlConnection(_db.CounnectionString))
            using (var cmd = new SqlCommand("sp_AddBook", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Title", book.Title);
                cmd.Parameters.AddWithValue("@AuthorId", book.AuthorId);
                cmd.Parameters.AddWithValue("@ISBN", book.ISBN);
                cmd.Parameters.AddWithValue("@TotalCopies", book.TotalCopies);
                cmd.Parameters.AddWithValue("@AvailableCopies", book.AvailableCopies);

                conn.Open();
                object result = cmd.ExecuteScalar();
                return Convert.ToInt32(result);
            }
        }

        public bool UpdateBook(Book book)
        {
            using(var conn = new SqlConnection(_db.CounnectionString))
            using (var cmd = new SqlCommand("sp_UpdateBook", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BookId", book.BookId);
                cmd.Parameters.AddWithValue("@Title", book.Title);
                cmd.Parameters.AddWithValue("@AuthorId", book.AuthorId);
                cmd.Parameters.AddWithValue("@ISBN", book.ISBN);
                cmd.Parameters.AddWithValue("@TotalCopies", book.TotalCopies);
                cmd.Parameters.AddWithValue("@AvailableCopies", book.AvailableCopies);

                conn.Open();
                object result = cmd.ExecuteScalar();
                return Convert.ToInt32(result) > 0;
            }
        }

        public bool DeleteBook(int bookId)
        {
            using(var conn = new SqlConnection(_db.CounnectionString))
            using(var cmd = new SqlCommand("sp_DeleteBook", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@BookId", bookId);

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 547)
                    {
                        throw new BusinessException("لا يمكن حذف الكتاب لوجود سجلات استعارة مرتبطة به (نشطة أو سابقة).");
                    }
                    throw;
                }
            }
        }
    }
}
