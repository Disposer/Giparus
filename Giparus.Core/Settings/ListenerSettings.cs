using System;
using System.IO;
using Newtonsoft.Json;

namespace Giparus.Core.Settings
{
    public class ListenerSettings
    {
        #region Constants
        private const string SETTINGS_PATH = "ListenerSettings.json";
        #endregion


        #region Properties
        public static ListenerSettings Instance { get; private set; }
        private static string SettingsPath { get { return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SETTINGS_PATH); } }

        public int Port { get; set; }
        public int BufferSize { get; set; }
        public string ConnectionString { get; set; }
        public string DbName { get; set; }
        #endregion


        #region .ctor
        static ListenerSettings()
        {
            if (!File.Exists(SettingsPath))
            {
                Instance = new ListenerSettings();
                Instance.Save();
            }
            else
            {
                var json = File.ReadAllText(SettingsPath);
                Instance = JsonConvert.DeserializeObject<ListenerSettings>(json);
            }
        }

        public ListenerSettings()
        {
            this.BufferSize = 2048;
            this.Port = 2020;
            this.ConnectionString = "mongodb://localhost";
            this.DbName = "ADB";
        }
        #endregion


        public void Save()
        {
            var json = JsonConvert.SerializeObject(Instance);
            File.WriteAllText(SettingsPath, json);
        }
    }
}
