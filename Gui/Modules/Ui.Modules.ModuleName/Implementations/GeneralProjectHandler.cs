using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.GrpcClientParser;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Services.ManifestHandler.Implementations;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Container;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.SvfPlayer_sharp;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.ViewModels;
using Ui.Modules.ModuleName.Views;
using Application = System.Windows.Application;

namespace Ui.Modules.ModuleName.Implementations
{
    public class GeneralProjectHandler : IProjectHandler
    {
        private readonly ILogger logger;
        private readonly IDialogSelector dialogSelector;
        private readonly ISettingsData settingsData;

        public GeneralProjectHandler(
            ILogger logger,
            IDialogSelector dialogSelector,
            ISettingsData settingsData)
        {
            this.logger = logger;
            this.settingsData = settingsData;
            this.dialogSelector = dialogSelector;
        }

        // TO DO: provide contains setting!!!
        public static AttributeList GetAttributeList(ISettingsStorageModel content)
        {
            return new AttributeList(
                GetListFromString(content.RIdentifier),
                GetListFromString(content.CIdentifier),
                GetListFromString(content.IIdentifier),
                GetListFromString(content.TIdentifier),
                GetListFromString(content.ICIdentifier),
                GetListFromString(content.ConIdentifier),
                content.UseContains);
        }

        public static List<string> GetLayers(IParsedResult result)
        {
            List<string> layerNames = new List<string>(result.Components
                .ToList()
                .Select(comp => comp.FunctionalAttributes.LayerName)
                .ToHashSet());
            return layerNames;
        }

        public string SelectJtagsToBeExported(List<string> jtags)
        {
            if (jtags.Count > 1)
            {
                SelectionView selectionView = new SelectionView();
                jtags.Insert(0, SelectionViewModel.ConsiderAllGeneral);
                ((SelectionViewModel)selectionView.DataContext).InitFields(jtags);
                selectionView.Topmost = true;
                selectionView.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (selectionView.Height / 2);
                selectionView.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (selectionView.Width / 2);
                selectionView.ShowDialog();
                return ((SelectionViewModel)selectionView.DataContext).SelectedItem;
            }
            else
            {
                return SelectionViewModel.ConsiderAllGeneral;
            }
        }

        public bool IsJtagIncludedForExport(string jtag, string selectedJtag)
        {
            return jtag.Equals(selectedJtag) || selectedJtag.Equals(SelectionViewModel.ConsiderAllGeneral);
        }

        public Stream GetBsdlContenAsStream(out string fileName, string jtagDevice = "")
        {
            if (!dialogSelector.OpenGenericDialog(
                DialogType.OPENFILE,
                TextRessources.SelectBsdlFile + (string.IsNullOrEmpty(jtagDevice) ? string.Empty : ": " + jtagDevice),
                "No valid BSDL file selected!",
                out string bsdlFile))
            {
                fileName = string.Empty;
                return null;
            }

            fileName = bsdlFile;

            try
            {
                return File.Open(bsdlFile, FileMode.Open);
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return null;
            }
        }

        public string GetSettingsDestinationPath()
        {
            if (!dialogSelector.OpenGenericDialog(
                DialogType.SAVEFILE,
                TextRessources.ExportSettingsFile,
                "No valid target to save selected!",
                out string fileName,
                TextRessources.SettingsFilter))
            {
                return string.Empty;
            }

            return fileName;
        }

        public string GetBasePath()
        {
            if (!dialogSelector.OpenGenericDialog(
                DialogType.OPENFOLDER,
                TextRessources.SelectSvfFolder,
                "No valid path selected",
                out string fileName))
            {
                return string.Empty;
            }

            return fileName;
        }

        public string GetJsonExportPath()
        {
            if (!dialogSelector.OpenGenericDialog(
                DialogType.SAVEFILE,
                TextRessources.ExportObjectData,
                "No valid target to save selected!",
                out string filePath,
                TextRessources.JsonFilter))
            {
                return string.Empty;
            }

            return filePath;
        }

        public void ExportSvfFiles(List<ISvfData> data)
        {
            bool result = true;
            int count = 0;
            List<string> notFound = new List<string>();
            List<string> noBoundaryScanCells = new List<string>();
            foreach (var svf in data)
            {
                if ((svf.Content != null && svf.Content.Count > 0) && File.Exists(svf.GetCompleteFilePath()))
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
                if (svf.Content != null && svf.Content.Count > 0)
                {
                    string path = svf.GetCompleteFilePath().Substring(
                        0, svf.GetCompleteFilePath().LastIndexOf("\\", StringComparison.OrdinalIgnoreCase));
                    if (!Directory.Exists(path))
                    {
                        if (!Directory.CreateDirectory(path).Exists)
                        {
                            continue;
                        }
                    }

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
                else if (!notFound.Contains(svf.PinName) && svf.PinNameNotFoundAtAll)
                {
                    notFound.Add(svf.PinName);
                }
                else if (!noBoundaryScanCells.Contains(svf.PinName) && !svf.PinNameNotFoundAtAll)
                {
                    noBoundaryScanCells.Add(svf.PinName);
                }
            }

            string notFoundPins = string.Join(", ", notFound);
            if (!string.IsNullOrEmpty(notFoundPins))
            {
                logger.LogMessage(
                    "The following "
                    + notFound.Count
                    + " PINS were not found for JTAG: "
                    + data[0].JtagName
                    + ": "
                    + notFoundPins,
                    LogCategory.WARNING);
            }

            notFoundPins = string.Join(", ", noBoundaryScanCells);
            if (!string.IsNullOrEmpty(notFoundPins))
            {
                logger.LogMessage(
                    "The following "
                    + noBoundaryScanCells.Count
                    + " PINS were found, but are no boundary scan cells for JTAG: "
                    + data[0].JtagName
                    + ": "
                    + notFoundPins,
                    LogCategory.WARNING);
            }

            if (result)
            {
                logger.LogMessage(
                    "Successfully exported "
                    + count
                    + " SVF files into location: "
                    + data[0].BasePath,
                    LogCategory.INFO);
            }
            else
            {
                logger.LogMessage(
                    "Exporting of "
                    + data.Count
                    + " SVF files into location: "
                    + data[0].BasePath
                    + " failed!",
                    LogCategory.ERROR);
            }
        }

        public void LogResults(List<string> layerNames, IParsedResult result)
        {
            string layers = string.Join(", ", layerNames);
            if (!string.IsNullOrEmpty(layers))
            {
                logger.LogMessage("Layers received: " + layers, LogCategory.INFO);
            }

            logger.LogMessage("Components received: " + result.Components.Count, LogCategory.INFO);
            logger.LogMessage("Nets received: " + result.Nets.Count, LogCategory.INFO);
            HashSet<IPinComponent> pins = new HashSet<IPinComponent>();
            foreach (var comp in result.Components)
            {
                foreach (var pin in comp.Connections)
                {
                    pins.Add(pin);
                }
            }

            logger.LogMessage("Pins received: " + pins.Count, LogCategory.INFO);
        }

        public bool CheckJtagDestinationLocation(string basePath, string jtag, string selectedJtags, List<string> allJtags)
        {
            string finalDestination = basePath + "\\" + jtag.Replace(":", "_");
            if (Directory.Exists(finalDestination))
            {
                return true;
            }

            try
            {
                Directory.CreateDirectory(finalDestination);
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return false;
            }

            return true;
        }

        public void ExportSettingsFileContent(byte[] data, string fileName, bool useOnlyJsonSettingsFileAsByteContent = false)
        {
            if (SaveContentIntoFile(data, fileName))
            {
                logger.LogMessage("Settings file was successfully exported to " + fileName, LogCategory.INFO);
            }
        }

        public async Task<string> GetOdbProjectFolder()
        {
            dialogSelector.OpenGenericDialog(
                DialogType.OPENFOLDER,
                TextRessources.SelectTheOdbProjectPath,
                "No valid folder to open selected!",
                out string path);
            return await Task.FromResult(path).ConfigureAwait(false);
        }

        public void ExportJsonProjectFileContent(byte[] data, string fileName)
        {
            Stopwatch sw = Stopwatch.StartNew();
            if (SaveContentIntoFile(data, fileName))
            {
                Application.Current.Dispatcher.Invoke(() => logger.LogMessage(
                    "JSON project file was successfully exported to " + fileName, LogCategory.INFO));
            }

            sw.Stop();
            Debug.WriteLine(TextRessources.ExportIntoJsonFile, sw.Elapsed.TotalMilliseconds);
        }

        public byte[] GetJsonProjectContent()
        {
            if (!dialogSelector.OpenGenericDialog(
                DialogType.OPENFILE,
                TextRessources.ImportDataFromFile,
                "No JSON file was selected for import!",
                out string filePath,
                TextRessources.JsonFilter))
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

        public async Task<bool> SaveProjectFile(
            Action<bool, bool, IProjectHandler> actionItemsSelect,
            string filename,
            IManifestHandler manifestHandler,
            IProjectLoadHandler projectLoadHandler,
            ISettingsHandler settingsHandler,
            IBomHandler bomHandler,
            ISvfHandler svfHandler)
        {
            manifestHandler.SetPathAndManifestOfZipfile(filename, true);
            logger.LogMessage("Successfully created new project file: " + filename, LogCategory.INFO);
            await projectLoadHandler.UpdateOdbProject().ConfigureAwait(false);
            await projectLoadHandler.HandleExportJsonProject(manifestHandler as IProjectHandler, false).ConfigureAwait(false);
            settingsHandler.HandleExportSettings(manifestHandler as IProjectHandler);
            bomHandler.SaveBomIntoProjectFile();
            string selection = SelectionViewModel.ConsiderAllProjectfile;
            if (manifestHandler.BsdlContent.Count > 1)
            {
                Thread thread = new Thread(() =>
                {
                    SelectionView selectionView = new SelectionView();
                    List<string> jtags = new List<string>
                        {
                            SelectionViewModel.ConsiderAllProjectfile,
                        };
                    foreach (var bsdl in manifestHandler.BsdlContent)
                    {
                        jtags.Add(bsdl.Key.JTAGName);
                    }

                    (selectionView.DataContext as SelectionViewModel).InitFields(
                        jtags, "Select JTAG related content to be saved into projectfile");
                    selectionView.Topmost = true;
                    selectionView.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (selectionView.Height / 2);
                    selectionView.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (selectionView.Width / 2);
                    selectionView.ShowDialog();
                    selection = (selectionView.DataContext as SelectionViewModel).SelectedItem;
                });
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                thread.Join();
            }

            if (manifestHandler.SaveAllBsdlContentsIntoProjectFile(selection))
            {
                svfHandler.SaveCreatedSvfFilesIntoProject(selection);
            }

            logger.LogMessage("New project file successfully saved and opened", LogCategory.INFO);
            actionItemsSelect(false, true, manifestHandler as IProjectHandler);
            return true;
        }

        public WrappedProgrammerSettings GetPgmSettings(
            string jtagDevice,
            WrappedProgrammerSettings defSettings,
            bool askForSvfSettings)
        {
            if (!askForSvfSettings)
            {
                return defSettings;
            }

            WrappedProgrammerSettings pgmSettings = null;
            Thread thread = new Thread(() =>
            {
                ProgrammerSettingsView view = new ProgrammerSettingsView();
                ProgrammerSettingsViewModel vm = view.DataContext as ProgrammerSettingsViewModel;
                vm.InitValues(defSettings);
                vm.SetJtagDevice(jtagDevice);
                view.Topmost = true;
                view.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (view.Height / 2);
                view.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (view.Width / 2);
                view.ShowDialog();
                pgmSettings = new WrappedProgrammerSettings()
                {
                    CableCompensation = vm.CableCompensation,
                    Frequency = vm.Frequency,
                    IoVoltage = vm.IoVoltage,
                    Ip = vm.IP,
                    Port = vm.Port,
                    SupplyVoltage = vm.SupplyVoltage,
                    Target = vm.Target,
                    Slot = vm.Slot,
                    Id = defSettings.Id,
                };
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return pgmSettings;
        }

        public byte[] GetSettingsFileContent(bool useJsonSettingsFile = false)
        {
            if (!dialogSelector.OpenGenericDialog(
                DialogType.OPENFILE,
                TextRessources.ImportSettingsFile,
                "No path selected for settings import!",
                out string path,
                TextRessources.SettingsFilter))
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
            List<string> list = new List<string>(identifier.Split(";"));
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

        private uint GetValueOfSetting(string title, uint defValue, string errorTitle)
        {
            uint valueToUse = defValue;
            string value = dialogSelector.OpenInputDialog(title, defValue.ToString(CultureInfo.CurrentCulture));
            if (!string.IsNullOrEmpty(value))
            {
                if (uint.TryParse(value, out uint newFreq))
                {
                    valueToUse = newFreq;
                }
                else
                {
                    logger.LogMessage("Invalid number entered for " + errorTitle + "! Using default value", LogCategory.WARNING);
                }
            }
            else
            {
                logger.LogMessage("No number entered for " + errorTitle + "! Using default value", LogCategory.WARNING);
            }

            return valueToUse;
        }
    }
}
