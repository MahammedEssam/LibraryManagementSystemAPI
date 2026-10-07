using Microsoft.Data.SqlClient;
using System.Data;
using LibraryManagement.DAL.Models;

namespace LibraryManagement.DAL
{
    public class AuthorDAL
    {
        private readonly DatabaseConnection _db;
        public AuthorDAL(DatabaseConnection db)
        {
            _db = db;
        }
        public List<Author> GetAllAuthors()
        {
            var authors = new List<Author>();
            using(var conn = new SqlConnection(_db.CounnectionString))
            {
                using(var cmd = new SqlCommand("sp_GetAllAuthors", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    conn.Open();
                    using(var reader  = cmd.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            authors.Add(new Author
                            {
                                AuthorId = Convert.ToInt32(reader["AuthorId"]),
                                Name = reader["Name"].ToString()!,
                                Nationality = reader["Nationality"] != DBNull.Value ? reader["Nationality"].ToString() : null
                            });
                        }
                    }
                }
            }
            return authors;
        }
        public Author? GetAuthorById(int authorId)
        {
            using (var conn = new SqlConnection(_db.CounnectionString))
            {
                using(var cmd = new SqlCommand("sp_GetAuthorById", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AuthorId", authorId);
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Author
                            {
                                AuthorId = Convert.ToInt32(reader["AuthorId"]),
                                Name = reader["Name"].ToString()!,
                                Nationality = reader["Nationality"] != DBNull.Value ? reader["Nationality"].ToString() : null
                            };
                        }
                    }
                }
            }
            return null;
        }
        public int AddAuthor(Author author)
        {
            using(var conn = new SqlConnection(_db.CounnectionString))
            {
                using (var cmd = new SqlCommand("sp_AddAuthor", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", author.Name);
                    cmd.Parameters.AddWithValue("@Nationality", (object?)author.Nationality ?? DBNull.Value);

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
        public bool UpdateAuthor(Author author)
        {
            using(var conn = new SqlConnection(_db.CounnectionString))
            {
                using (var cmd = new SqlCommand("sp_UpdateAuthor", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@AuthorId", author.AuthorId);
                    cmd.Parameters.AddWithValue("@Name", author.Name);
                    cmd.Parameters.AddWithValue("@Nationality", (object?)author.Nationality ?? DBNull.Value);

                    conn.Open();
                    return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
                }

            }
        }
        public bool DeleteAuthor(int authorId)
        {
            using(var conn = new SqlConnection(_db.CounnectionString))
            using(var cmd = new SqlCommand("sp_DeleteAuthor", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@AuthorId", authorId);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // Check for books by Author to prevent deletion
        public bool HasBooks(int authorId)
        {
            using(var conn = new SqlConnection(_db.CounnectionString))
            using(var cmd = new SqlCommand("SELECT COUNT(1) FROM Books WHERE AuthorId = @AuthorId", conn))
            {
                cmd.Parameters.AddWithValue("@AuthorId", authorId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }
    }
}
