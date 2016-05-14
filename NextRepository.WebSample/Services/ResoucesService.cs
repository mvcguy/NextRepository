using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NextRepository.WebSample.Services
{
    public class ResoucesService
    {
        private Assembly _assembly;
        public virtual string GetResourceString(string resId)
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
