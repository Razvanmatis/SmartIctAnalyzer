using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Helper
{
    public class SettingsStorageManager : ISettingsStorageManager
    {
        private const string FILENAME = "settingsStorage.settings";
        private static string filePath = Directory.GetCurrentDirectory() + "\\" + FILENAME;
        private static StorageColor rDefColor = new StorageColor(245, 245, 220);
        private static StorageColor cDefColor = new StorageColor(0, 191, 255);
        private static StorageColor iDefColor = new StorageColor(255, 255, 0);
        private static StorageColor tpDefColor = new StorageColor(50, 205, 50);
        private static StorageColor icDefColor = new StorageColor(0, 255, 255);
        private static StorageColor compDefColor = new StorageColor(255, 255, 255);
        private static StorageColor conDefColor = new StorageColor(236, 108, 255);
        private static string ipdef = "127.0.0.1";
        private static ISettingsStorageContent defStorage = GetDefaultStorageEntry();

        public ISettingsStorageContent GetStorageContent()
        {
            if (!File.Exists(filePath))
            {
                SaveStorageContent(defStorage);
                return defStorage;
            }
            else
            {
                return GetStorageContentFromFile(filePath);
            }
        }

        public void SaveStorageContent(ISettingsStorageContent content)
        {
            SaveStorageContentIntoFile(content, filePath);
        }

        public void ImportStorageContent(string filePathToUse)
        {
            ISettingsStorageContent content = GetStorageContentFromFile(filePathToUse);
            SaveStorageContent(content);
        }

        public void ExportStorageContent(string filePathToUse)
        {
            SaveStorageContentIntoFile(GetStorageContent(), filePathToUse);
        }

        private static ISettingsStorageContent GetStorageContentFromFile(string fileToUse)
        {
            string content = string.Empty;
            try
            {
                content = File.ReadAllText(fileToUse);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error reading out the settings file: " + fileToUse + ": " + e.Message);
                return defStorage;
            }

            if (string.IsNullOrEmpty(content))
            {
                Debug.WriteLine("Settings file is empty! Using default settings");
                return defStorage;
            }

            SettingsStorageContent settingValue = null;
            try
            {
                settingValue = JsonConvert.DeserializeObject<SettingsStorageContent>(content);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error parsing settings file: " + e.Message);
            }

            if (settingValue == null)
            {
                settingValue = (SettingsStorageContent)defStorage;
            }
            else
            {
                if (settingValue.RColor == null)
                {
                    settingValue.RColor = rDefColor;
                }

                if (settingValue.CColor == null)
                {
                    settingValue.CColor = cDefColor;
                }

                if (settingValue.IColor == null)
                {
                    settingValue.IColor = iDefColor;
                }

                if (settingValue.TColor == null)
                {
                    settingValue.TColor = tpDefColor;
                }

                if (settingValue.ICColor == null)
                {
                    settingValue.ICColor = icDefColor;
                }

                if (settingValue.CompColor == null)
                {
                    settingValue.CompColor = compDefColor;
                }

                if (settingValue.ConColor == null)
                {
                    settingValue.ConColor = conDefColor;
                }

                if (string.IsNullOrEmpty(settingValue.Ip))
                {
                    settingValue.Ip = ipdef;
                }
            }

            return settingValue;
        }

        private static void SaveStorageContentIntoFile(ISettingsStorageContent content, string filePathToUse)
        {
            bool fileOk = true;
            if (!File.Exists(filePathToUse))
            {
                try
                {
                    File.Create(filePathToUse).Close();
                }
                catch (Exception e)
                {
                    fileOk = false;
                    Debug.WriteLine(e.Message);
                }
            }

            if (fileOk)
            {
                try
                {
                    string jsonString = JsonConvert.SerializeObject(content, Formatting.Indented);
                    File.WriteAllText(filePathToUse, jsonString);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error at serializing the settings object into JSON file: " + e.Message);
                }
            }
        }

        private static ISettingsStorageContent GetDefaultStorageEntry()
        {
            return new SettingsStorageContent(
                "r",
                "c",
                "l",
                "tp;testpoint",
                "j;ic",
                "x",
                12,
                rDefColor,
                cDefColor,
                iDefColor,
                tpDefColor,
                icDefColor,
                compDefColor,
                conDefColor,
                false,
                false,
                false,
                false,
                false,
                true,
                true,
                false,
                "gnd",
                "3v;5v;vdd",
                "jtag",
                string.Empty,
                string.Empty,
                string.Empty,
                ipdef);
        }
    }
}
