﻿using System.Data;
using System.Data.SqlClient;

namespace NextRepository.Common
{
    public class MsSqlDatabase : Database
    {
        public override IDbConnection GetConnection(string connectionString)
        {
            return new SqlConnection(connectionString);
        }
    }
}