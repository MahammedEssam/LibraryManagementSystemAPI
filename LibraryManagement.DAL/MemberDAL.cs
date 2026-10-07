using Microsoft.Data.SqlClient;
using System.Data;
using LibraryManagement.DAL.Models;
using LibraryManagement.DAL.Exceptions;

namespace LibraryManagement.DAL
{
    public class MemberDAL
    {
        private readonly DatabaseConnection _db;

        public MemberDAL(DatabaseConnection db)
        {
            _db = db;
        }

        public List<Member> GetAllMembers()
        {
            var members = new List<Member>();
            using (var conn = new SqlConnection(_db.CounnectionString))
            using (var cmd = new SqlCommand("sp_GetAllMembers", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        members.Add(new Member
                        {
                            MemberId = Convert.ToInt32(reader["MemberId"]),
                            Name = reader["Name"].ToString()!,
                            Email = reader["Email"].ToString()!,
                            PhoneNumber = reader["PhoneNumber"] != DBNull.Value ? reader["PhoneNumber"].ToString() : null,
                            JoinDate = Convert.ToDateTime(reader["JoinDate"])
                        });
                    }
                }
            }
            return members;
        }

        public Member? GetMemberById(int memberId)
        {
            using (var conn = new SqlConnection(_db.CounnectionString))
            using (var cmd = new SqlCommand("sp_GetMemberById", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MemberId", memberId);
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new Member
                        {
                            MemberId = Convert.ToInt32(reader["MemberId"]),
                            Name = reader["Name"].ToString()!,
                            Email = reader["Email"].ToString()!,
                            PhoneNumber = reader["PhoneNumber"] != DBNull.Value ? reader["PhoneNumber"].ToString() : null,
                            JoinDate = Convert.ToDateTime(reader["JoinDate"])
                        };
                    }
                }
            }
            return null;
        }

        public bool IsEmailExists(string email, int excludeMemberId = 0)
        {
            using (var conn = new SqlConnection(_db.CounnectionString))
            using (var cmd = new SqlCommand("SELECT COUNT(1) FROM Members WHERE Email = @Email AND MemberId <> @ExcludeId", conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@ExcludeId", excludeMemberId);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public (bool IsSuccess, string Message, int NewId) AddMember(Member member)
        {
            try
            {
                using (var conn = new SqlConnection(_db.CounnectionString))
                using (var cmd = new SqlCommand("sp_AddMember", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Name", member.Name);
                    cmd.Parameters.AddWithValue("@Email", member.Email);
                    cmd.Parameters.AddWithValue("@PhoneNumber", (object?)member.PhoneNumber ?? DBNull.Value);

                    conn.Open();
                    int newId = Convert.ToInt32(cmd.ExecuteScalar());
                    return (true, "Member registered successfully.", newId);
                }
            }
            catch (SqlException ex)
            {
                // 2627 أو 2601 هي أرقام خطأ تكرار المفتاح الفريد (Unique Constraint Violation) في SQL Server
                if (ex.Number == 2627 || ex.Number == 2601 || ex.Message.Contains("Email is already registered"))
                {
                    return (false, "Email is already registered in the system.", 0);
                }

                // إلقاء الاستثناء أو تسجيله حسب نظام Logging المعمول به
                throw;
            }
        }

        public bool UpdateMember(Member member)
        {
            using (var conn = new SqlConnection(_db.CounnectionString))
            using (var cmd = new SqlCommand("sp_UpdateMember", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MemberId", member.MemberId);
                cmd.Parameters.AddWithValue("@Name", member.Name);
                cmd.Parameters.AddWithValue("@Email", member.Email);
                cmd.Parameters.AddWithValue("@PhoneNumber", (object?)member.PhoneNumber ?? DBNull.Value);

                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        public bool DeleteMember(int memberId)
        {
            using (var conn = new SqlConnection(_db.CounnectionString))
            using (var cmd = new SqlCommand("sp_DeleteMember", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@MemberId", memberId);

                try
                {
                    conn.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
                catch (SqlException ex)
                {
                    // 547 = Foreign Key Constraint Violation in SQL Server
                    if (ex.Number == 547)
                    {
                        throw new BusinessException("لا يمكن حذف العضو لوجود سجلات استعارة مرتبطة به (نشطة أو سابقة).");
                    }
                    throw;
                }
            }
        }
    }
}