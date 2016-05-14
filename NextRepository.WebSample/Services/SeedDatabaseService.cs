using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Repository.MsSql;
using Repository.MySql;

namespace NextRepository.WebSample.Services
{
    public class SeedDatabaseService
    {
        private readonly IMySqlRepository _mySqlRepository;
        private readonly IMsSqlRepository _msSqlRepository;
        private readonly ResoucesService _resoucesService;
        private static bool _mySqlSeeded = false;
        private static bool _msSqlSeeded = false;

        public SeedDatabaseService(IMySqlRepository mySqlRepository,IMsSqlRepository msSqlRepository,ResoucesService resoucesService)
        {
            _mySqlRepository = mySqlRepository;
            _msSqlRepository = msSqlRepository;
            _resoucesService = resoucesService;
        }

        public void DropCreateDatabaseMySql()
        {
            if (_mySqlSeeded) return;

            var sql = "SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = 'NextDatalayerWeb'";

            var record = _mySqlRepository.Query<dynamic>(sql).FirstOrDefault();

            if (record != null)
            {
                _mySqlSeeded = true;
                return;
            }

            sql = _resoucesService.GetResourceString("NextRepository.WebSample.Resources.CreateDatabaseMySql.sql");
            _mySqlRepository.NonQuery(sql);
            _mySqlSeeded = true;
        }

        public void DropCreateDatabaseMsSql()
        {
            if (_msSqlSeeded) return;

            const string sql = "SELECT name FROM master.dbo.sysdatabases WHERE name = N'NextDataLayerWeb'";

            var record = _msSqlRepository.Query<dynamic>(sql).FirstOrDefault();

            if (record != null)
            {
                _msSqlSeeded = true;
                return;
            }

            var createDbScript = _resoucesService.GetResourceString("NextRepository.WebSample.Resources.CreateDatabaseMsSql1.sql");
            var createTblScript = _resoucesService.GetResourceString("NextRepository.WebSample.Resources.CreateDatabaseMsSql2.sql");
            var seedScript = _resoucesService.GetResourceString("NextRepository.WebSample.Resources.CreateDatabaseMsSql3.sql");

            var x=_msSqlRepository.NonQuery(createDbScript, useTransaction: false);
            var y=_msSqlRepository.NonQuery(createTblScript, useTransaction: false);
            var z=_msSqlRepository.NonQuery(seedScript, useTransaction: false);

            _msSqlSeeded = true;
        }
        
    }
}
