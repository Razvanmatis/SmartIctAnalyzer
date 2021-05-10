using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Interfaces.Gui;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.Model;

namespace Ui.Modules.ModuleName.Implementations
{
    public class ProMikSettingsStorageManager : ISettingsStorageManager
    {
        public static readonly string SETTINGDBFILEPATH = Directory.GetCurrentDirectory() + "\\" + SETTINGDBNAME;
        private const string SETTINGDBNAME = "settings.db";
        private readonly ILogger logger;
        private readonly IProjectHandler projectHandler;
        private readonly ISettingsData settingsData;

        public ProMikSettingsStorageManager(ILogger logger, IProjectHandler projectHandler, ISettingsData settingsData)
        {
            this.logger = logger;
            this.settingsData = settingsData;
            this.projectHandler = projectHandler;
        }

        public byte[] GetSettingsContent()
        {
            if (File.Exists(SETTINGDBFILEPATH))
            {
                return ((GeneralProjectHandler)projectHandler).GetBytesOfFile(SETTINGDBFILEPATH);
            }
            else
            {
                logger.LogMessage("No valid settings.db file found!", LogCategory.WARNING);
                return null;
            }
        }

        public ISettingsStorageModel GetStorageContent()
        {
            return new SettingsStorageModel(
                GetTextFromList(settingsData.ResistorChars),
                GetTextFromList(settingsData.CapacitorChars),
                GetTextFromList(settingsData.InductionChars),
                GetTextFromList(settingsData.TestpointChars),
                GetTextFromList(settingsData.ICChars),
                GetTextFromList(settingsData.ConnectorChars),
                settingsData.FontSizeNames,
                new StorageColor(settingsData.ResistorColor.R, settingsData.ResistorColor.G, settingsData.ResistorColor.B),
                new StorageColor(settingsData.CapacitorColor.R, settingsData.CapacitorColor.G, settingsData.CapacitorColor.B),
                new StorageColor(settingsData.InductionColor.R, settingsData.InductionColor.G, settingsData.InductionColor.B),
                new StorageColor(settingsData.TestpointColor.R, settingsData.TestpointColor.G, settingsData.TestpointColor.B),
                new StorageColor(settingsData.ICColor.R, settingsData.ICColor.G, settingsData.ICColor.B),
                new StorageColor(settingsData.CompColor.R, settingsData.CompColor.G, settingsData.CompColor.B),
                new StorageColor(settingsData.ConnectorColor.R, settingsData.ConnectorColor.G, settingsData.ConnectorColor.B),
                settingsData.ResistorNamesChecked,
                settingsData.CapacitorNamesChecked,
                settingsData.InductionNamesChecked,
                settingsData.TestpointNamesChecked,
                settingsData.ICNamesChecked,
                settingsData.CompNamesChecked,
                settingsData.ConnectorNamesChecked,
                settingsData.NetNamesChecked,
                settingsData.GndNetIdentifier,
                settingsData.PowerNetIdentifier,
                settingsData.JTAGNetIdentifier,
                settingsData.GndNetBlacklist,
                settingsData.PowerNetBlacklist,
                settingsData.JTAGNetBlacklist,
                settingsData.IPAddress,
                settingsData.ShowButtons,
                settingsData.PgmIp,
                settingsData.PgmPort,
                settingsData.SupplyVoltageMv,
                settingsData.IoVoltageMv,
                settingsData.Steps,
                settingsData.JTAGPinIdentifier);
        }

        public void ImportStorageContent(byte[] data)
        {
            try
            {
                HandleCreationOfNewFile(data);
            }
            catch (CustomCollectionException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }
        }

        public byte[] IsValidSettingsFile(string fileName)
        {
            if (fileName.ToLower(CultureInfo.CurrentCulture).EndsWith(SETTINGDBNAME, StringComparison.InvariantCulture))
            {
                return ((GeneralProjectHandler)projectHandler).GetBytesOfFile(fileName);
            }
            else
            {
                return null;
            }
        }

        public void SaveStorageContent(ISettingsStorageModel content)
        {
        }

        private static string GetTextFromList(IList<string> values)
        {
            string text = string.Empty;
            foreach (string val in values)
            {
                text += val + ";";
            }

            return text[0..^1];
        }

        /// <summary>
        /// HandleCreationOfNewFile
        /// </summary>
        /// <param name="data">data</param>
        /// <exception cref="CustomCollectionException">Ignore.</exception>
        private static void HandleCreationOfNewFile(byte[] data)
        {
            if (File.Exists(SETTINGDBFILEPATH))
            {
                try
                {
                    File.Delete(SETTINGDBFILEPATH);
                }
                catch (Exception e)
                {
                    throw new CustomCollectionException(e.Message);
                }
            }

            try
            {
                File.WriteAllBytes(SETTINGDBFILEPATH, data);
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }
    }
}
