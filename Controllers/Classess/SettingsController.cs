using System;
using System.IO;
using Newtonsoft.Json;

namespace ARCA_WPF_F.Controllers.Classess
{
    internal class SettingsController
    {
        [JsonProperty]
        private string IP;
        [JsonProperty]
        private bool IsDebugOpen;
        private static readonly string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Broniev", "ARCA-Saves", "B-v.1");
        private static readonly string filePath = Path.Combine(directoryPath, "settings.json");

        public SettingsController() { }
        public void SaveIP(string IP)
        {
           this.IP = IP;
        }

        public string GetIP() { 
            return this.IP; 
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static SettingsController FromJson(string json)
        {
            return JsonConvert.DeserializeObject<SettingsController>(json);
        }

        public void SaveToFile()
        {
            string json = this.ToJson();
            string directoryPath = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(filePath, json);
        }

        public static SettingsController LoadFromFile()
        {
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                return FromJson(json);
            }
            else
            {
                return new SettingsController();
            }
        }

        public bool Debug
        {
            get
            {
                return this.IsDebugOpen;
            }
            set
            {
                this.IsDebugOpen = value;
            }
        }
    }
}
