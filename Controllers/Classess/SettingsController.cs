using System;
using System.IO;
using Newtonsoft.Json;

namespace ARCA_WPF_F.Controllers.Classess
{
    public class SettingsController
    {
        public string IP { get; set; } = "192.168.0.1"; 
        public bool IsDebugOpen { get; set; } = false;

        private static readonly string directoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Broniev", "ARCA-Saves", "B-v.1");
        private static readonly string filePath = Path.Combine(directoryPath, "settings.json");

        public SettingsController() { }

        public void SaveToFile()
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                // Formatting.Indented makes the JSON file human-readable
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                // Prevent app crash if file is locked or access is denied
                System.Windows.MessageBox.Show($"Error saving settings: {ex.Message}", "Settings Error");
            }
        }

        public static SettingsController LoadFromFile()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var settings = JsonConvert.DeserializeObject<SettingsController>(json);

                    // Return loaded settings, or new instance if deserialization failed (null)
                    return settings ?? new SettingsController();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error loading settings: {ex.Message}\nDefault settings will be used.", "Settings Error");
            }

            // Return default settings if file doesn't exist or is corrupted
            return new SettingsController();
        }
    }
}