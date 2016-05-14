using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace NextRepository.WebSample.Services
{
    public class AppQueriesService
    {
        private readonly ResoucesService _resoucesService;
        public ConcurrentDictionary<string, dynamic> MsSqlQueries { get; private set; }
        public ConcurrentDictionary<string, dynamic> MySqlQueries { get; private set; }

        private string _mySqlJson;
        private string _msSqlJson;

        public AppQueriesService(ResoucesService resoucesService)
        {
            _resoucesService = resoucesService;
            MsSqlQueries = new ConcurrentDictionary<string, dynamic>();
            MySqlQueries = new ConcurrentDictionary<string, dynamic>();
        }

        public void Init()
        {
            LoadJson();
            MapJson();
        }

        protected virtual void MapJson()
        {
            if (!MsSqlQueries.Any())
            {
                MsSqlQueries = JsonConvert.DeserializeObject<ConcurrentDictionary<string, dynamic>>(_msSqlJson);
            }


            if (!MySqlQueries.Any())
            {
                MySqlQueries = JsonConvert.DeserializeObject<ConcurrentDictionary<string, dynamic>>(_mySqlJson);
            }
           
        }

        protected virtual void LoadJson()
        {
            if (string.IsNullOrWhiteSpace(_mySqlJson))
            {
                _mySqlJson = _resoucesService.GetResourceString("NextRepository.WebSample.Resources.AppQueriesMySql.json");
            }
            if (string.IsNullOrWhiteSpace(_msSqlJson))
            {
                _msSqlJson = _resoucesService.GetResourceString("NextRepository.WebSample.Resources.AppQueriesMsSql.json");
            }
        }

    }
}
