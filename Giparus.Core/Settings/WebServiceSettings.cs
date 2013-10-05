using System;
using System.IO;
using Newtonsoft.Json;

namespace Giparus.Core.Settings
{
    public class WebServiceSettings
    {
        #region Constants
        private const string SETTINGS_PATH = "WebServiceSettings.json";
        #endregion


        #region Properties
        public static WebServiceSettings Instance { get; private set; }
        private static string SettingsPath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SETTINGS_PATH); } }

        public bool IndexEnsured { get; set;  }
        public string WebServiceHost { get; set; }      
        #endregion


        #region .ctor
        static WebServiceSettings()
        {
            if (!File.Exists(SettingsPath))
            {
                Instance = new WebServiceSettings();
                Instance.Save();
            }
            else
            {
                var json = File.ReadAllText(SettingsPath);
                Instance = JsonConvert.DeserializeObject<WebServiceSettings>(json);
            }
        }

        public WebServiceSettings()
        {
            this.WebServiceHost = "http://localhost:1234";
            this.IndexEnsured = false;
        }
        #endregion


        public void Save()
        {
            var json = JsonConvert.SerializeObject(Instance);
            File.WriteAllText(SettingsPath, json);
        }
    }
}
