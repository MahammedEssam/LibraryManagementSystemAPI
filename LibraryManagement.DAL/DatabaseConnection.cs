
namespace LibraryManagement.DAL
{
    public class DatabaseConnection
    {
        private readonly string _connectionString;
        public DatabaseConnection(string connectionString)
        {
            _connectionString = connectionString;
        }
        public string CounnectionString => _connectionString;
    }
}
