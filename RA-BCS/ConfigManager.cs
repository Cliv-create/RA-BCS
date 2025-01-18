using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// TODO: Remove unused usings.
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RA_BCS
{
    [JsonSourceGenerationOptions(WriteIndented = true)]
    [JsonSerializable(typeof(Dictionary<string, string>))]
    internal partial class ConfigJsonContext : JsonSerializerContext
    {
    }

    internal static class ConfigManager
    {
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        private static Dictionary<string, string> _config;

        /// <summary>
        /// Loads config.json from folder, containing executable. config.json gets deserialized in Dictionary<string, string>. To get config values use Get property.
        /// </summary>
        /// <exception cref="Exception">Exception if config.json was not found.</exception>
        public static void LoadConfig()
        {
            Console.WriteLine("Loading config");
            if (File.Exists(ConfigFilePath))
            {
                Console.WriteLine("Found file!");
                string json = File.ReadAllText(ConfigFilePath);
                Console.WriteLine("Read file contents!");
                _config = JsonSerializer.Deserialize(json, ConfigJsonContext.Default.DictionaryStringString);
                Console.WriteLine($"Deserialized contents successfully!");
            }
            else
            {
                throw new Exception("config.json not found!");
                // _config = new Dictionary<string, string>();
            }
        }

        /// <summary>
        /// Generates config.json in folder, containing executable. Config will have all the settings field. Mandatory settings: token, yt-dlp_path.
        /// </summary>
        public static void GenerateInitialConfig()
        {
            Console.WriteLine($"Writing initial config...");
            _config = new Dictionary<string, string>();
            _config.Add("telegram_token", "TOKEN_HERE");
            _config.Add("yt-dlp_path", "PATH_HERE");
            _config.Add("yt-dlp_download_path", "DOWNLOAD_FOLDER_PATH_HERE");
            _config.Add("downloaded_files_destination", "DOWNLOADED_FILES_DESTINATION_PATH_HERE");
            string json = JsonSerializer.Serialize(_config, ConfigJsonContext.Default.DictionaryStringString);
            File.WriteAllText(ConfigFilePath, json);
            Console.WriteLine($"Successfully wrote initial config!");
        }

        /// <summary>
        /// Uses TryGetValue to get value from config.json. Can generate exception.
        /// </summary>
        /// <param name="key">Dictionary key.</param>
        /// <param name="defaultValue">null by default. If set, will return if nothning was found.</param>
        /// <returns>Found value, or null (defaultValue if set) if nothning was found.</returns>
        public static string Get(string key, string defaultValue = null)
        {
            return _config.TryGetValue(key, out string value) ? value : defaultValue;
        }

        // Config is meant to be defined by user manually.
        // It is not meant to be changed in runtime, but to be changed manually.
        // TODO: If runtime config change capabilities will be added - uncomment this code.

        /*
        public static void SaveConfig()
        {
            string json = JsonSerializer.Serialize(_config, ConfigJsonContext.Default.DictionaryStringString);
            File.WriteAllText(ConfigFilePath, json);
        }
        */

        /*
        public static void Set(string key, string value)
        {
            _config[key] = value;
            SaveConfig();
        }
        */
    }
}
