using LibraryManagement.DAL;

namespace LibraryManagement.BLL
{
    public class HealthBLL
    {
        private readonly HealthDAL _healthDAL;
        public HealthBLL (HealthDAL healthDAL)
        {
            _healthDAL = healthDAL;
        }
        public (bool IsSuccess, string Message, string DbStatus) GetSystemStatus()
        {
            bool IsDbHealthy = _healthDAL.CheckDatabaseHealth(out string dbMessage);

            if (IsDbHealthy)
            {
                return (true, "Library Management API is running smoothly.", dbMessage);
            }

            return (false, "Library Management API is running, but DB check failed", dbMessage);
        }
    }
}
