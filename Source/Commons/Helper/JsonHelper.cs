using Modules.Communication.Params;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace FZ4P.Commons.Helper
{
    public class JsonHelper
    {
        public static void Save<TParamType>(string path, TParamType param)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(param, options);
            File.WriteAllText(path, json);
        }
        public static TParamType Load<TParamType>(string path) where TParamType : class
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<TParamType>(json);
        }
    }
}
