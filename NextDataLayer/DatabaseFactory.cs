using System;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace NextDataLayer
{
   public class DatabaseFactory
    {
        public static Database CreateDatabaseConnection(Type databaseType)
        {
            return (Database)Activator.CreateInstance(databaseType);
        }
    }
}
