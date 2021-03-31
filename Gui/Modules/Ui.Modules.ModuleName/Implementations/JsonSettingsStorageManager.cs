using System;
using System.IO;
using System.Security;
using System.Text;
using Interfaces.Gui;
using Newtonsoft.Json;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.Model;

namespace Ui.Modules.ModuleName.Implementations
{
    public class JsonSettingsStorageManager : ISettingsStorageManager
    {
        public const string JtagDef = "jtag";
        public const string PowerDef = "3v;5v;vdd";
        public const string GndDef = "gnd";
        public const string ConDef = "x";
        public const string IcDef = "j;ic";
        public const string TpDef = "tp;testpoint";
        public const string IDef = "l";
        public const int FontDef = 12;
        public const string CDef = "c";
        public const string RDef = "r";
        public const string Ipdef = "10.91.30.184";
        public const string PgmIpDef = "192.168.3.11";
        public const string StepsDef = "-1";
        public const uint PgmPortDef = 15504;
        public const uint SupplyVoltageMvDef = 12000;
        public const uint IoVoltageMvDef = 5000;
        public static readonly StorageColor RDefColor = new StorageColor(245, 245, 220);
        public static readonly StorageColor CDefColor = new StorageColor(0, 191, 255);
        public static readonly StorageColor IDefColor = new StorageColor(255, 255, 0);
        public static readonly StorageColor TpDefColor = new StorageColor(50, 205, 50);
        public static readonly StorageColor IcDefColor = new StorageColor(0, 255, 255);
        public static readonly StorageColor CompDefColor = new StorageColor(255, 255, 255);
        public static readonly StorageColor ConDefColor = new StorageColor(236, 108, 255);
        public static readonly ISettingsStorageModel DefStorage = GetDefaultStorageEntry();
        private const string FILENAME = "settingsStorage.settings";
        private static readonly string FilePath = Directory.GetCurrentDirectory() + "\\" + FILENAME;
        private readonly ILogger logger;

        public JsonSettingsStorageManager(ILogger logger)
        {
            this.logger = logger;
        }

        public ISettingsStorageModel GetStorageContent()
        {
            if (!File.Exists(FilePath))
            {
                SaveStorageContent(DefStorage);
                return DefStorage;
            }
            else
            {
                return GetStorageContentFromData(Encoding.ASCII.GetString(GetBytesOfFile(FilePath)));
            }
        }

        public void SaveStorageContent(ISettingsStorageModel content)
        {
            SaveStorageContentIntoFile(content, FilePath);
        }

        public void ImportStorageContent(byte[] data)
        {
            ISettingsStorageModel content = GetStorageContentFromData(Encoding.ASCII.GetString(data));
            SaveStorageContent(content);
        }

        public byte[] GetSettingsContent()
        {
            return Encoding.ASCII.GetBytes(GetContentAsString(GetStorageContent()));
        }

        public byte[] IsValidSettingsFile(string fileName)
        {
            byte[] content = GetBytesOfFile(fileName);
            if (content == null)
            {
                return null;
            }

            SettingsStorageModel settingValue;
            try
            {
                settingValue = JsonConvert.DeserializeObject<SettingsStorageModel>(Encoding.ASCII.GetString(content));
            }
            catch (JsonException)
            {
                return null;
            }

            if ((settingValue == null) || (settingValue.CColor == null && settingValue.CIdentifier == null && settingValue.CompColor == null))
            {
                return null;
            }
            else
            {
                return content;
            }
        }

        private static ISettingsStorageModel GetDefaultStorageEntry()
        {
            return new SettingsStorageModel(
                RDef,
                CDef,
                IDef,
                TpDef,
                IcDef,
                ConDef,
                FontDef,
                RDefColor,
                CDefColor,
                IDefColor,
                TpDefColor,
                IcDefColor,
                CompDefColor,
                ConDefColor,
                false,
                false,
                false,
                false,
                false,
                true,
                true,
                false,
                GndDef,
                PowerDef,
                JtagDef,
                string.Empty,
                string.Empty,
                string.Empty,
                Ipdef,
                false,
                PgmIpDef,
                PgmPortDef,
                SupplyVoltageMvDef,
                IoVoltageMvDef,
                StepsDef);
        }

        private byte[] GetBytesOfFile(string fileToUse)
        {
            byte[] content = null;
            try
            {
                content = File.ReadAllBytes(fileToUse);
            }
            catch (IOException e)
            {
                logger.LogMessage("Error reading out the settings file: " + fileToUse + ": " + e.Message, LogCategory.ERROR);
            }
            catch (ArgumentException e)
            {
                logger.LogMessage("Error reading out the settings file: " + fileToUse + ": " + e.Message, LogCategory.ERROR);
            }
            catch (NotSupportedException e)
            {
                logger.LogMessage("Error reading out the settings file: " + fileToUse + ": " + e.Message, LogCategory.ERROR);
            }
            catch (UnauthorizedAccessException e)
            {
                logger.LogMessage("Error reading out the settings file: " + fileToUse + ": " + e.Message, LogCategory.ERROR);
            }
            catch (SecurityException e)
            {
                logger.LogMessage("Error reading out the settings file: " + fileToUse + ": " + e.Message, LogCategory.ERROR);
            }

            return content;
        }

        private string GetContentAsString(ISettingsStorageModel settingsStorageContent)
        {
            try
            {
                return JsonConvert.SerializeObject(settingsStorageContent, Formatting.Indented);
            }
            catch (JsonException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return string.Empty;
            }
        }

        private ISettingsStorageModel GetStorageContentFromData(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                logger.LogMessage("Settings file is empty! Using default settings", LogCategory.ERROR);
                return DefStorage;
            }

            SettingsStorageModel settingValue = null;
            try
            {
                settingValue = JsonConvert.DeserializeObject<SettingsStorageModel>(content);
            }
            catch (JsonException e)
            {
                logger.LogMessage("Error parsing settings file: " + e.Message, LogCategory.ERROR);
            }

            if (settingValue == null)
            {
                settingValue = (SettingsStorageModel)DefStorage;
            }
            else
            {
                if (settingValue.RColor == null)
                {
                    settingValue.RColor = RDefColor;
                }

                if (settingValue.CColor == null)
                {
                    settingValue.CColor = CDefColor;
                }

                if (settingValue.IColor == null)
                {
                    settingValue.IColor = IDefColor;
                }

                if (settingValue.TColor == null)
                {
                    settingValue.TColor = TpDefColor;
                }

                if (settingValue.ICColor == null)
                {
                    settingValue.ICColor = IcDefColor;
                }

                if (settingValue.CompColor == null)
                {
                    settingValue.CompColor = CompDefColor;
                }

                if (settingValue.ConColor == null)
                {
                    settingValue.ConColor = ConDefColor;
                }

                if (string.IsNullOrEmpty(settingValue.Ip))
                {
                    settingValue.Ip = Ipdef;
                }

                if (settingValue.Steps == null)
                {
                    settingValue.Steps = StepsDef;
                }
            }

            return settingValue;
        }

        private void SaveStorageContentIntoFile(ISettingsStorageModel content, string filePathToUse)
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
