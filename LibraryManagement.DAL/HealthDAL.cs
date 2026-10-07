using Microsoft.Data.SqlClient;
using System.Data;

namespace LibraryManagement.DAL
{
    public class HealthDAL
    {
        private readonly DatabaseConnection _db;
        public HealthDAL(DatabaseConnection db)
        {
            _db = db;
        }
        public bool CheckDatabaseHealth(out string dbMessage)
        {
            dbMessage = string.Empty;
            try
            {
                using(var conn = new SqlConnection(_db.CounnectionString))
                {
                    using(var cmd = new SqlCommand("sp_CheckServerHealth", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        conn.Open();

                        using(var reader = cmd.ExecuteReader())
                        {
                            if(reader.Read())
                            {
                                dbMessage = reader["Message"].ToString()!;
                                return Convert.ToInt32(reader["IsHealthy"]) == 1;
                            }
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                dbMessage = ex.Message;
                return false;
            }
        }
    }
}
