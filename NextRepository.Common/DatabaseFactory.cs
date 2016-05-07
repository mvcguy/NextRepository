using System;

namespace NextRepository.Common
{
   public class DatabaseFactory
    {
        public static Database CreateDatabaseConnection(Type databaseType)
        {
            return (Database)Activator.CreateInstance(databaseType);
        }
    }
}
