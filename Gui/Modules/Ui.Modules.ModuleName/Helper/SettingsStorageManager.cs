using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using System.Text;
using Interfaces.Gui;
using Newtonsoft.Json;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Helper
{
    public class SettingsStorageManager : ISettingsStorageManager
    {
        private const string FILENAME = "settingsStorage.settings";
        private static string jtagDef = "jtag";
        private static string powerDef = "3v;5v;vdd";
        private static string gndDef = "gnd";
        private static string conDef = "x";
        private static string icDef = "j;ic";
        private static string tpDef = "tp;testpoint";
        private static string iDef = "l";
        private static string cDef = "c";
        private static string rDef = "r";
        private static string filePath = Directory.GetCurrentDirectory() + "\\" + FILENAME;
        private static StorageColor rDefColor = new StorageColor(245, 245, 220);
        private static StorageColor cDefColor = new StorageColor(0, 191, 255);
        private static StorageColor iDefColor = new StorageColor(255, 255, 0);
        private static StorageColor tpDefColor = new StorageColor(50, 205, 50);
        private static StorageColor icDefColor = new StorageColor(0, 255, 255);
        private static StorageColor compDefColor = new StorageColor(255, 255, 255);
        private static StorageColor conDefColor = new StorageColor(236, 108, 255);
        private static string ipdef = "10.91.30.184";
        private static ISettingsStorageContent defStorage = GetDefaultStorageEntry();
        private ILogger logger;

        public SettingsStorageManager(ILogger logger)
        {
            this.logger = logger;
        }

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

        private static ISettingsStorageContent GetDefaultStorageEntry()
        {
            return new SettingsStorageContent(
                rDef,
                cDef,
                iDef,
                tpDef,
                icDef,
                conDef,
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
                gndDef,
                powerDef,
                jtagDef,
                string.Empty,
                string.Empty,
                string.Empty,
                ipdef);
        }

        private ISettingsStorageContent GetStorageContentFromFile(string fileToUse)
        {
            string content = string.Empty;
            try
            {
                content = File.ReadAllText(fileToUse);
            }
            catch (IOException e)
            {
                logger.LogMessage("Error reading out the settings file: " + fileToUse + ": " + e.Message, LogCategory.ERROR);
                return defStorage;
            }
            catch (ArgumentException e)
            {
                logger.LogMessage("Error reading out the settings file: " + fileToUse + ": " + e.Message, LogCategory.ERROR);
                return defStorage;
            }
            catch (NotSupportedException e)
            {
                logger.LogMessage("Error reading out the settings file: " + fileToUse + ": " + e.Message, LogCategory.ERROR);
                return defStorage;
            }
            catch (UnauthorizedAccessException e)
            {
                logger.LogMessage("Error reading out the settings file: " + fileToUse + ": " + e.Message, LogCategory.ERROR);
                return defStorage;
            }
            catch (SecurityException e)
            {
                logger.LogMessage("Error reading out the settings file: " + fileToUse + ": " + e.Message, LogCategory.ERROR);
                return defStorage;
            }

            if (string.IsNullOrEmpty(content))
            {
                logger.LogMessage("Settings file is empty! Using default settings", LogCategory.ERROR);
                return defStorage;
            }

            SettingsStorageContent settingValue = null;
            try
            {
                settingValue = JsonConvert.DeserializeObject<SettingsStorageContent>(content);
            }
            catch (JsonException e)
            {
                logger.LogMessage("Error parsing settings file: " + e.Message, LogCategory.ERROR);
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

        private void SaveStorageContentIntoFile(ISettingsStorageContent content, string filePathToUse)
        {
            bool fileOk = true;
            if (!File.Exists(filePathToUse))
            {
                try
                {
                    File.Create(filePathToUse).Close();
                }
                catch (IOException e)
                {
                    fileOk = false;
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                }
                catch (ArgumentException e)
                {
                    fileOk = false;
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                }
                catch (NotSupportedException e)
                {
                    fileOk = false;
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                }
                catch (UnauthorizedAccessException e)
                {
                    fileOk = false;
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                }
            }

            if (fileOk)
            {
                try
                {
                    string jsonString = JsonConvert.SerializeObject(content, Formatting.Indented);
                    File.WriteAllText(filePathToUse, jsonString);
                }
                catch (IOException e)
                {
                    logger.LogMessage("Error at serializing the settings object into JSON file: " + e.Message, LogCategory.ERROR);
                }
                catch (ArgumentException e)
                {
                    logger.LogMessage("Error at serializing the settings object into JSON file: " + e.Message, LogCategory.ERROR);
                }
                catch (UnauthorizedAccessException e)
                {
                    logger.LogMessage("Error at serializing the settings object into JSON file: " + e.Message, LogCategory.ERROR);
                }
                catch (NotSupportedException e)
                {
                    logger.LogMessage("Error at serializing the settings object into JSON file: " + e.Message, LogCategory.ERROR);
                }
                catch (SecurityException e)
                {
                    logger.LogMessage("Error at serializing the settings object into JSON file: " + e.Message, LogCategory.ERROR);
                }
            }
        }
    }
}
