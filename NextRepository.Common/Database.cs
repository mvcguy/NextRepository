using System.Data;

namespace NextRepository.Common
{
    public abstract class Database
    {
        public abstract IDbConnection GetConnection(string connectionString);
    }
}