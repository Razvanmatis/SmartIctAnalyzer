using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using GrpcClientParser.Interfaces;
using Interfaces.Gui;
using Interfaces.PcbInvestigator;
using SVFHelper.Interfaces;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class GeneralProjectHandler : IProjectHandler
    {
        private readonly ILogger logger;
        private readonly IDialogSelector dialogSelector;

        public GeneralProjectHandler(ILogger logger, IDialogSelector dialogSelector)
        {
            this.logger = logger;
            this.dialogSelector = dialogSelector;
        }

        public static AttributeList GetAttributeList(ISettingsStorageModel content)
        {
            return new AttributeList(
                GetListFromString(content.RIdentifier),
                GetListFromString(content.CIdentifier),
                GetListFromString(content.IIdentifier),
                GetListFromString(content.TIdentifier),
                GetListFromString(content.ICIdentifier),
                GetListFromString(content.ConIdentifier));
        }

        public static List<string> GetLayers(IParsedResult result)
        {
            List<string> layerNames = new List<string>();
            foreach (var comp in result.Components)
            {
                if (!layerNames.Contains(comp.FunctionalAttributes.LayerName))
                {
                    layerNames.Add(comp.FunctionalAttributes.LayerName);
                }
            }

            return layerNames;
        }

        public void ExportSvfFiles(List<ISvfData> data)
        {
            bool result = true;
            int count = 0;
            List<string> notFound = new List<string>();
            foreach (var svf in data)
            {
                if (!string.IsNullOrEmpty(svf.SvfRequestContent) && File.Exists(svf.GetCompleteFilePath()))
                {
                    try
                    {
                        DeleteFile(svf.GetCompleteFilePath());
                    }
                    catch (CustomCollectionException e)
                    {
                        logger.LogMessage(e.Message, LogCategory.ERROR);
                    }
                }
            }

            foreach (var svf in data)
            {
                if (!string.IsNullOrEmpty(svf.SvfRequestContent))
                {
                    bool res = SaveContentIntoFile(
                    Encoding.ASCII.GetBytes(svf.GetCompleteContentAsString()), svf.GetCompleteFilePath());
                    if (!res)
                    {
                        result = false;
                    }
                    else
                    {
                        count++;
                    }
                }
                else if (!notFound.Contains(svf.PinName))
                {
                    notFound.Add(svf.PinName);
                }
            }

            string notFoundPins = string.Empty;
            foreach (var pin in notFound)
            {
                notFoundPins += pin + ", ";
            }

            if (!string.IsNullOrEmpty(notFoundPins))
            {
                logger.LogMessage("The following " + notFound.Count + " PINS were not found for JTAG: " + data[0].JtagName + ": " + notFoundPins[0..^2], LogCategory.WARNING);
            }

            if (result)
            {
                logger.LogMessage("Successfully exported " + count + " SVF files into location: " + data[0].BasePath, LogCategory.INFO);
            }
            else
            {
                logger.LogMessage("Exporting of " + data.Count + " SVF files into location: " + data[0].BasePath + " failed!", LogCategory.ERROR);
            }
        }

        public void LogResults(List<string> layerNames, IParsedResult result)
        {
            string layers = string.Empty;
            foreach (var layer in layerNames)
            {
                layers += layer + ", ";
            }

            if (!string.IsNullOrEmpty(layers))
            {
                logger.LogMessage("Layers received: " + layers[0..^2], LogCategory.INFO);
            }

            logger.LogMessage("Components received: " + result.Components.Count, LogCategory.INFO);
            logger.LogMessage("Nets received: " + result.Nets.Count, LogCategory.INFO);
            List<IPinComponent> pins = new List<IPinComponent>();
            foreach (var comp in result.Components)
            {
                foreach (var pin in comp.Connections)
                {
                    if (!pins.Contains(pin))
                    {
                        pins.Add(pin);
                    }
                }
            }

            logger.LogMessage("Pins received: " + pins.Count, LogCategory.INFO);
        }

        public void ExportSettingsFileContent(byte[] data, string fileName)
        {
            if (SaveContentIntoFile(data, fileName))
            {
                logger.LogMessage("Settings file was successfully exported to " + fileName, LogCategory.INFO);
            }
        }

        public async Task<string> GetOdbProjectFolder()
        {
            string path = string.Empty;
            dialogSelector.OpenGenericDialog(DialogType.OPENFOLDER, TextRessources.SelectTheOdbProjectPath, "No valid folder to open selected!", out path);
            return await Task.FromResult(path).ConfigureAwait(false);
        }

        public void ExportJsonProjectFileContent(byte[] data, string fileName)
        {
            if (SaveContentIntoFile(data, fileName))
            {
                Application.Current.Dispatcher.Invoke(() => logger.LogMessage("JSON project file was successfully exported to " + fileName, LogCategory.INFO));
            }
        }

        public byte[] GetJsonProjectContent()
        {
            string filePath = string.Empty;
            if (!dialogSelector.OpenGenericDialog(DialogType.OPENFILE, TextRessources.ImportDataFromFile, "No JSON file was selected for import!", out filePath, TextRessources.JsonFilter))
            {
                return null;
            }

            return GetBytesOfFile(filePath);
        }

        public byte[] GetBytesOfFile(string fileToUse)
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

        public byte[] GetSettingsFileContent()
        {
            string path = string.Empty;
            if (!dialogSelector.OpenGenericDialog(DialogType.OPENFILE, TextRessources.ImportSettingsFile, "No path selected for settings import!", out path, TextRessources.SettingsFilter))
            {
                return null;
            }

            return GetBytesOfFile(path);
        }

        public string GetBomData()
        {
            return string.Empty;
        }

        public BomSettings GetBomSettings()
        {
            return null;
        }

        private static List<string> GetListFromString(string identifier)
        {
            List<string> list = new List<string>();
            foreach (var text in identifier.Split(";"))
            {
                list.Add(text);
            }

            return list;
        }

        /// <summary>
        /// DeleteFile
        /// </summary>
        /// <param name="fileName">fileName</param>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        private static void DeleteFile(string fileName)
        {
            try
            {
                File.Delete(fileName);
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        private bool SaveContentIntoFile(byte[] data, string fileName)
        {
            bool fileOk = true;
            if (!File.Exists(fileName))
            {
                try
                {
                    File.Create(fileName).Close();
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
                    File.WriteAllBytes(fileName, data);
                    return true;
                }
                catch (IOException e)
                {
                    logger.LogMessage("Error at writing data into file: " + e.Message, LogCategory.ERROR);
                }
                catch (ArgumentException e)
                {
                    logger.LogMessage("Error at writing data into file: " + e.Message, LogCategory.ERROR);
                }
                catch (UnauthorizedAccessException e)
                {
                    logger.LogMessage("Error at writing data into file: " + e.Message, LogCategory.ERROR);
                }
                catch (NotSupportedException e)
                {
                    logger.LogMessage("Error at writing data into file: " + e.Message, LogCategory.ERROR);
                }
                catch (SecurityException e)
                {
                    logger.LogMessage("Error at writing data into file: " + e.Message, LogCategory.ERROR);
                }
            }

            return false;
        }
    }
}
