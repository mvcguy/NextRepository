using System.Data;

namespace NextDataLayer
{
    public abstract class Database
    {
        public abstract IDbConnection GetConnection(string connectionString);
    }
}