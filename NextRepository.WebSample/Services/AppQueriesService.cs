using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace NextRepository.WebSample.Services
{
    public class AppQueriesService
    {
        public ConcurrentDictionary<string, dynamic> MsSqlQueries { get; private set; }
        public ConcurrentDictionary<string, dynamic> MySqlQueries { get; private set; }

        private string _mySqlJson;
        private string _msSqlJson;
        private Assembly _assembly;

        public AppQueriesService()
        {
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
                _mySqlJson = GetResourceString("NextRepository.WebSample.Resources.AppQueriesMySql.json");
            }
            if (string.IsNullOrWhiteSpace(_msSqlJson))
            {
                _msSqlJson = GetResourceString("NextRepository.WebSample.Resources.AppQueriesMsSql.json");
            }
        }

        protected virtual string GetResourceString(string resId)
        {
            if (_assembly == null)
            {
                _assembly = Assembly.GetExecutingAssembly();
            }

            var resourceStream = _assembly.GetManifestResourceStream(resId);
            if (resourceStream == null) return string.Empty;

            using (var reader = new StreamReader(resourceStream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }


    }
}
