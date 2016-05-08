using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Repository.MySql;

namespace NextRepository.WebSample.Services
{
    public class SeedDatabaseService
    {
        private readonly IMySqlRepository _mySqlRepository;
        private static bool _seeded = false;

        public SeedDatabaseService(IMySqlRepository mySqlRepository)
        {
            _mySqlRepository = mySqlRepository;
        }

        public void DropCreateDatabase()
        {
            if (_seeded) return;

            var sql = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'NextDatalayerWeb'";

            var record = _mySqlRepository.Query<dynamic>(sql, CommandType.Text).FirstOrDefault();

            if (record != null)
            {
                _seeded = true;
                return;
            }

            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream("NextRepository.WebSample.Resources.CreateDatabase.sql");
            if (resourceStream != null)
                using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
                {
                    sql = reader.ReadToEnd();
                }
            _mySqlRepository.NonQuery(sql, CommandType.Text);
            _seeded = true;
        }

    }
}
