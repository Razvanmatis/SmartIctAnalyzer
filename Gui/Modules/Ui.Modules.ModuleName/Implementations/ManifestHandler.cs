using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ionic.Zip;
using Newtonsoft.Json;
using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Services.ManifestHandler.Implementations;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Container;
using ProMik.Svf.Contracts;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.ViewModels;
using Ui.Modules.ModuleName.Views;

namespace Ui.Modules.ModuleName.Implementations
{
    public class ManifestHandler : ManifestActionsHandler, IManifestHandler
    {
        private const string SVFPROJ = ".svfproj";
        private static readonly string TEMPPATH = Directory.GetCurrentDirectory() + "\\tempPathForManifestHandling\\";
        private readonly IDialogSelector dialogSelector;
        private readonly ISettingsData settingsData;

        public ManifestHandler(ILogger logger, IDialogSelector dialogSelector, ISettingsData settingsData)
            : base(logger)
        {
            this.settingsData = settingsData;
            this.dialogSelector = dialogSelector;
        }

        public Dictionary<BsdlContainer, byte[]> BsdlContent { get; } = new Dictionary<BsdlContainer, byte[]>();

        public override void ResetValues(bool skipFolderDeletion = false)
        {
            base.ResetValues(skipFolderDeletion);
            BsdlContent.Clear();
            if (skipFolderDeletion)
            {
                return;
            }

            if (Directory.Exists(TEMPPATH))
            {
                try
                {
                    DeleteDirectory(TEMPPATH);
                }
                catch (CustomCollectionException e)
                {
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                }
            }
        }

        public override void ExportSettingsFileContent(byte[] data, string fileName, bool useOnlyJsonSettingsFileAsByteContent = false)
        {
            if (!useOnlyJsonSettingsFileAsByteContent)
            {
                var jsonObject = GetSettingsFileContentAsJson();
                if (jsonObject != null)
                {
                    HandleSavingOfZipEntry(Manifest.SETTINGSLOCDEF + SettingJsonFileName, jsonObject, Manifest.SETTINGSLOCDEF + SettingJsonFileName);
                }
            }

            base.ExportSettingsFileContent(data, fileName, useOnlyJsonSettingsFileAsByteContent);
        }

        public override Stream GetBsdlContenAsStream(out string fileName, string jtagDevice = "")
        {
            string pathToUse;
            if (!string.IsNullOrEmpty(jtagDevice))
            {
                pathToUse = GetBsdlPathToUse(jtagDevice);
                if (string.IsNullOrEmpty(pathToUse))
                {
                    pathToUse = HandleImportOfBsdl(jtagDevice);
                }
            }
            else
            {
                pathToUse = HandleSelectionOfExistingFiles();
            }

            fileName = pathToUse;

            if (!string.IsNullOrEmpty(pathToUse))
            {
                return GetZipStream(pathToUse);
            }
            else
            {
                return null;
            }
        }

        public string GetProjectSelectionPath()
        {
            dialogSelector.OpenGenericDialog(
                DialogType.OPENFILE,
                TextRessources.SelectProjectFileForOpening,
                "No path selected for project import!",
                out string path);
            return path;
        }

        public async Task<string> GetOdbProjectFolder()
        {
            if (!IsManifestHandlingActive())
            {
                return string.Empty;
            }

            if (Directory.Exists(TEMPPATH))
            {
                try
                {
                    DeleteDirectory(TEMPPATH);
                }
                catch (CustomCollectionException e)
                {
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                    return string.Empty;
                }
            }

            try
            {
                return await GetExtractedOdbPath().ConfigureAwait(false);
            }
            catch (CustomCollectionException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return string.Empty;
            }
        }

        public string SelectJtagsToBeExported(List<string> jtags)
        {
            if (jtags.Count > 1)
            {
                SelectionView selectionView = new SelectionView();
                jtags.Insert(0, SelectionViewModel.ConsiderEachProjectfile);
                jtags.Insert(0, SelectionViewModel.ConsiderAllProjectfile);
                ((SelectionViewModel)selectionView.DataContext).InitFields(jtags);
                selectionView.Topmost = true;
                selectionView.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (selectionView.Height / 2);
                selectionView.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (selectionView.Width / 2);
                selectionView.ShowDialog();
                return ((SelectionViewModel)selectionView.DataContext).SelectedItem;
            }
            else
            {
                return SelectionViewModel.ConsiderAllProjectfile;
            }
        }

        public string GetBasePath()
        {
            return Manifest.SVFDEF;
        }

        public Manifest GetActuallyLoadedManifest()
        {
            return Manifest;
        }

        public bool IsJtagIncludedForExport(string jtag, string selectedJtag)
        {
            return jtag.Equals(selectedJtag)
                || selectedJtag.Equals(SelectionViewModel.ConsiderAllProjectfile)
                || selectedJtag.Equals(SelectionViewModel.ConsiderEachProjectfile);
        }

        public string GetJsonExportPath()
        {
            return Manifest.JSONPROJECTDEF.Substring(
                Manifest.JSONPROJECTDEF.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase) + 1);
        }

        public string GetSettingsDestinationPath()
        {
            return Manifest.SETTINGSDEF.Substring(
                Manifest.SETTINGSDEF.LastIndexOf("/", StringComparison.InvariantCultureIgnoreCase) + 1);
        }

        public void UpdatePgmSettings(string jtagDevice, ProgrammerSettings pgmSettings)
        {
            if (Manifest != null && Manifest.BSDL != null && Manifest.BSDL.Count > 0)
            {
                foreach (var bsdl in Manifest.BSDL)
                {
                    if (bsdl.JTAGName.Equals(jtagDevice))
                    {
                        bsdl.ProgrammerSettings = pgmSettings;
                        UpdateManifestInZipfile(Manifest);
                        break;
                    }
                }
            }

            var bsdlContainer = BsdlContent.Select(entry => entry.Key).FirstOrDefault(bsdl => bsdl.JTAGName.Equals(jtagDevice));
            if (bsdlContainer != null)
            {
                bsdlContainer.ProgrammerSettings = pgmSettings;
            }
        }

        public WrappedProgrammerSettings GetPgmSettings(
            string jtagDevice,
            WrappedProgrammerSettings defSettings,
            bool askForSettings)
        {
            WrappedProgrammerSettings settingsToUse = defSettings;
            var bsdlContent = Manifest.BSDL.FirstOrDefault(
                bsdl => bsdl.JTAGName.Equals(jtagDevice) && bsdl.ProgrammerSettings != null);
            if (bsdlContent != null)
            {
                settingsToUse = new WrappedProgrammerSettings()
                {
                    CableCompensation = bsdlContent.ProgrammerSettings.CableCompensation,
                    Frequency = bsdlContent.ProgrammerSettings.Frequency,
                    Id = bsdlContent.ProgrammerSettings.Id,
                    IoVoltage = bsdlContent.ProgrammerSettings.IoVoltage,
                    Ip = defSettings.Ip,
                    Port = bsdlContent.ProgrammerSettings.Port,
                    SupplyVoltage = bsdlContent.ProgrammerSettings.SupplyVoltage,
                    Target = bsdlContent.ProgrammerSettings.Target,
                };
            }

            if (!askForSettings)
            {
                return settingsToUse;
            }

            WrappedProgrammerSettings pgmSettings = null;
            Thread thread = new Thread(() =>
            {
                ProgrammerSettingsView view = new ProgrammerSettingsView();
                ProgrammerSettingsViewModel vm = view.DataContext as ProgrammerSettingsViewModel;
                vm.InitValues(settingsToUse);
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

        public bool CheckJtagDestinationLocation(string basePath, string jtagName, string selectedJtag, List<string> allJtags)
        {
            if (selectedJtag.Equals(SelectionViewModel.ConsiderAllProjectfile))
            {
                return true;
            }

            if (selectedJtag.Equals(jtagName))
            {
                return ExistingProjectContainsNoOtherSvf(jtagName);
            }

            string jtagReplaced = jtagName.Replace(":", "_");
            bool state = false;
            if (ManifestProjectFilePath.Replace(SVFPROJ, string.Empty)
                .EndsWith(jtagReplaced, StringComparison.OrdinalIgnoreCase))
            {
                state = true;
            }
            else if (IsProjectFileAlreadyExisting(jtagReplaced, allJtags, out string newProjectFile))
            {
                ChangeExistingProjectFileSelection(newProjectFile);
                state = true;
            }
            else if (CheckIfCurrentProjectFileNeedsJustToBeRenamed(allJtags, jtagReplaced))
            {
                state = true;
            }
            else if (CreateNewCopyOfProject(newProjectFile))
            {
                state = true;
            }

            return state;
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
            if (!filename.Equals(ManifestProjectFilePath))
            {
                try
                {
                    await Task.Run(() => File.Copy(ManifestProjectFilePath, filename)).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                    return false;
                }

                SetPathAndManifestOfZipfile(filename, false);
                actionItemsSelect(false, true, this);
                logger.LogMessage("Projectfile successfully saved to " + filename, LogCategory.INFO);
            }

            return true;
        }

        public void UpdateBsdlParameter(
            string jtagName,
            uint boundaryScanLength,
            uint instructionLength,
            uint idCodeRegister,
            uint idCodeInstruction,
            uint preloadInstruction)
        {
            if (Manifest != null && Manifest.BSDL != null && Manifest.BSDL.Count > 0)
            {
                foreach (var bsdl in Manifest.BSDL)
                {
                    if (bsdl.JTAGName.Equals(jtagName))
                    {
                        bsdl.ScanChainLength = boundaryScanLength;
                        bsdl.InstructionLength = instructionLength;
                        bsdl.JtagIdCode = idCodeRegister;
                        bsdl.IdCodeInstr = idCodeInstruction;
                        bsdl.PreloadInstr = preloadInstruction;
                        break;
                    }
                }
            }

            if (BsdlContent != null)
            {
                foreach (var bsdl in BsdlContent.Keys)
                {
                    if (bsdl.JTAGName.Equals(jtagName))
                    {
                        bsdl.ScanChainLength = boundaryScanLength;
                        bsdl.InstructionLength = instructionLength;
                        bsdl.JtagIdCode = idCodeRegister;
                        bsdl.IdCodeInstr = idCodeInstruction;
                        bsdl.PreloadInstr = preloadInstruction;
                        break;
                    }
                }
            }
        }

        public bool AddBsdlContentToDict(string bsdlFile, string jtagDevice, out BsdlContainer bsdl, out byte[] data)
        {
            string fileName = Manifest.BSDLDEF + bsdlFile[(bsdlFile.LastIndexOf("\\", StringComparison.InvariantCulture) + 1)..];
            bsdl = new BsdlContainer() { JTAGName = jtagDevice, BSDLFileName = fileName };
            try
            {
                data = File.ReadAllBytes(bsdlFile);
                BsdlContent.Add(bsdl, data);
                return true;
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                data = null;
                return false;
            }
        }

        public bool SaveAllBsdlContentsIntoProjectFile(string jtagSelection)
        {
            foreach (var (bsdl, data) in BsdlContent)
            {
                if (!jtagSelection.Equals(SelectionViewModel.ConsiderAllProjectfile) && !jtagSelection.Equals(bsdl.JTAGName))
                {
                    continue;
                }

                if (!SaveBsdlContentIntoProjectFile(bsdl, data))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// CreateZipFile
        /// </summary>
        /// <param name="path">path</param>
        /// <param name="target">target</param>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        private static void CreateZipFile(string path, string target)
        {
            try
            {
                using ZipFile zip = new ZipFile();
                zip.Password = PASSWORD;
                zip.AddDirectory(path);
                zip.Save(target);
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        /// <summary>
        /// CreateDirectory
        /// </summary>
        /// <param name="path">path</param>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        private static void CreateDirectory(string path)
        {
            try
            {
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        /// <summary>
        /// DeleteDirectory
        /// </summary>
        /// <param name="path">path</param>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        private static void DeleteDirectory(string path)
        {
            try
            {
                Directory.Delete(path, true);
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        /// <summary>
        /// CreateNewFileAndWriteDataInto
        /// </summary>
        /// <param name="path">path</param>
        /// <param name="dataToWrite">dataToWrite</param>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        private static void CreateNewFileAndWriteDataInto(string path, string dataToWrite)
        {
            try
            {
                using FileStream stream = File.Create(path);
                if (!string.IsNullOrEmpty(dataToWrite))
                {
                    stream.Write(ASCIIEncoding.ASCII.GetBytes(dataToWrite));
                }

                stream.Close();
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        private static bool IsAlreadyOneSvfPathExisting(BsdlContainer bsdl, List<BsdlContainer> bsdlList)
        {
            return bsdlList.FirstOrDefault(bs => bs != bsdl && bs.SvfPath != null) != null;
        }

        private byte[] GetSettingsFileContentAsJson()
        {
            SettingsContent content = new SettingsContent()
            {
                General = new GeneralSettings()
                {
                    CapacitorIdentifier = string.Join(";", settingsData.CapacitorChars),
                    ConnectorIdentifier = string.Join(";", settingsData.ConnectorChars),
                    IcIdentifier = string.Join(";", settingsData.ICChars),
                    InductionIdentifer = string.Join(";", settingsData.InductionChars),
                    ResistorIdentifier = string.Join(";", settingsData.ResistorChars),
                    TestPointIdentifier = string.Join(";", settingsData.TestpointChars),
                    UseContains = settingsData.UseContains,
                },
                GrpcServer = new GrpcServerSettings()
                {
                    IpAddress = settingsData.IPAddress,
                },
                PcbInvestigator = new PcbInvestigatorSettings()
                {
                    StepsToReadOut = settingsData.Steps,
                },
                TestCoverage = new TestCoverageSettings()
                {
                    GndBlacklist = settingsData.GndNetBlacklist,
                    GndIdentifier = settingsData.GndNetIdentifier,
                    JtagBlacklist = settingsData.JTAGNetBlacklist,
                    JtagIdentifier = settingsData.JTAGPinIdentifier,
                    PowerBlacklist = settingsData.PowerNetBlacklist,
                    PowerIdentifier = settingsData.PowerNetIdentifier,
                },
                Programmer = new WrappedProgrammerSettings()
                {
                    CableCompensation = settingsData.CableCompensation,
                    Frequency = settingsData.Frequency,
                    Id = 0,
                    IoVoltage = settingsData.IoVoltageMv,
                    Ip = settingsData.PgmIp,
                    Port = settingsData.PgmPort,
                    Slot = (int)settingsData.Slot,
                    Target = (int)settingsData.Target,
                    SupplyVoltage = settingsData.SupplyVoltageMv,
                },
            };

            try
            {
                return Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(content, Formatting.Indented));
            }
            catch (JsonException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return null;
            }
        }

        /// <summary>
        /// GetExtractedOdbPath
        /// </summary>
        /// <returns>the path</returns>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        private async Task<string> GetExtractedOdbPath()
        {
            return await Task.Run<string>(() =>
            {
                try
                {
                    if (!Directory.Exists(TEMPPATH))
                    {
                        Directory.CreateDirectory(TEMPPATH);
                    }

                    if (!Directory.Exists(TEMPPATH + Manifest.OdbProject))
                    {
                        Directory.CreateDirectory(TEMPPATH + Manifest.OdbProject);
                    }

                    bool found = false;
                    using (ZipFile archive = ZipFile.Read(ManifestProjectFilePath))
                    {
                        if (ZipFile.CheckZipPassword(ManifestProjectFilePath, PASSWORD))
                        {
                            archive.Password = PASSWORD;
                        }
                        else
                        {
                            logger.LogMessage("Used other password protected projectfile!", LogCategory.ERROR);
                            return string.Empty;
                        }

                        foreach (var entry in archive.Entries)
                        {
                            if (entry.FileName.Contains(
                                Manifest.OdbProject)
                            || entry.FileName.Replace("/", "\\").Contains(Manifest.OdbProject))
                            {
                                found = true;
                                if (!entry.FileName.Equals(Manifest.OdbProject))
                                {
                                    entry.Extract(TEMPPATH);
                                }
                            }
                        }
                    }

                    if (found)
                    {
                        return TEMPPATH + Manifest.OdbProject;
                    }
                    else
                    {
                        if (Directory.Exists(TEMPPATH))
                        {
                            Directory.Delete(TEMPPATH, true);
                        }

                        return string.Empty;
                    }
                }
                catch (Exception e)
                {
                    throw new CustomCollectionException(e.Message);
                }
            }).ConfigureAwait(false);
        }

        private bool CheckIfCurrentProjectFileNeedsJustToBeRenamed(List<string> allJtags, string jtagToUse)
        {
            bool found = allJtags.FirstOrDefault(jtag =>
                ManifestProjectFilePath.Replace(SVFPROJ, string.Empty)
                .EndsWith(jtag.Replace(":", "_"), StringComparison.OrdinalIgnoreCase)) != null;
            if (!found)
            {
                string newProjectFileName = ManifestProjectFilePath.Replace(SVFPROJ, string.Empty) + "_" + jtagToUse + SVFPROJ;
                try
                {
                    File.Move(ManifestProjectFilePath, newProjectFileName);
                    logger.LogMessage("Currently loaded project file was renamed to: " + newProjectFileName, LogCategory.INFO);
                }
                catch (Exception e)
                {
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                    return false;
                }

                SetPathAndManifestOfZipfile(newProjectFileName, false);
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ExistingProjectContainsNoOtherSvf(string jtagName)
        {
            List<BsdlContainer> containerToDelete = new List<BsdlContainer>(
                Manifest.BSDL.Where(bsdl => !bsdl.JTAGName.Equals(jtagName)));
            if (containerToDelete.Count > 0)
            {
                try
                {
                    if (!DeleteAllSvfEntries())
                    {
                        logger.LogMessage("Error deleting existing SVF files from project!", LogCategory.ERROR);
                        return false;
                    }
                }
                catch (CustomCollectionException e)
                {
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                    return false;
                }

                foreach (var bsdl in containerToDelete)
                {
                    DeleteContentFromZip(bsdl.BSDLFileName);
                    Manifest.BSDL.Remove(bsdl);
                }

                UpdateManifestInZipfile(Manifest);
            }

            return true;
        }

        private bool IsProjectFileAlreadyExisting(string jtagReplaced, List<string> allJtags, out string newProjectFile)
        {
            string actualProjectFile = ManifestProjectFilePath;
            foreach (var jtag in allJtags)
            {
                actualProjectFile = actualProjectFile.Replace(SVFPROJ, string.Empty)
                    .Replace("_" + jtag.Replace(":", "_"), string.Empty);
            }

            actualProjectFile += "_" + jtagReplaced + SVFPROJ;
            newProjectFile = actualProjectFile;
            return File.Exists(actualProjectFile);
        }

        private string HandleSelectionOfExistingFiles()
        {
            string result = string.Empty;
            if (Manifest.BSDL == null || Manifest.BSDL.Count == 0)
            {
                return result;
            }

            if (Manifest.BSDL.Count == 1)
            {
                logger.LogMessage("Selected the only BSDL file of project: " + Manifest.BSDL[0].BSDLFileName, LogCategory.INFO);
                return Manifest.BSDL[0].BSDLFileName;
            }

            string files = string.Empty;
            for (int id = 1; id <= Manifest.BSDL.Count; id++)
            {
                files += id + ": " + Manifest.BSDL[id - 1].JTAGName + ", ";
            }

            files = files[0..^2];

            string value = dialogSelector.OpenInputDialog("Choose BSDL file: " + files);
            if (!string.IsNullOrEmpty(value) && int.TryParse(value, out int number))
            {
                if (number > 0 && Manifest.BSDL.Count >= number)
                {
                    return Manifest.BSDL[number - 1].BSDLFileName;
                }
                else
                {
                    logger.LogMessage("Entered number is out of range of all BSDL files!", LogCategory.ERROR);
                    return result;
                }
            }
            else
            {
                logger.LogMessage("No valid input entered!", LogCategory.ERROR);
                return result;
            }
        }

        private string HandleImportOfBsdl(string jtagDevice)
        {
            if (dialogSelector == null)
            {
                return string.Empty;
            }

            if (!dialogSelector.OpenGenericDialog(
                DialogType.OPENFILE,
                TextRessources.SelectBsdlFile + ": " + jtagDevice,
                "No valid BSDL file selected!",
                out string bsdlFile))
            {
                return string.Empty;
            }

            if (!AddBsdlContentToDict(bsdlFile, jtagDevice, out BsdlContainer bsdl, out byte[] data))
            {
                return string.Empty;
            }

            if (!SaveBsdlContentIntoProjectFile(bsdl, data))
            {
                return string.Empty;
            }

            return bsdl.BSDLFileName;
        }

        private bool ChangeExistingProjectFileSelection(string newProjectFile)
        {
            if (newProjectFile.Equals(ManifestProjectFilePath))
            {
                return true;
            }

            if (!File.Exists(newProjectFile))
            {
                logger.LogMessage("Project to open does not exist: " + newProjectFile, LogCategory.ERROR);
                return false;
            }

            SetPathAndManifestOfZipfile(newProjectFile, false);
            logger.LogMessage("Changed actual projectfile to use to: " + newProjectFile, LogCategory.INFO);
            return true;
        }

        private bool CreateNewCopyOfProject(string newProjectFileName)
        {
            if (!File.Exists(ManifestProjectFilePath))
            {
                logger.LogMessage("Project file to copy does not exist: " + ManifestProjectFilePath, LogCategory.ERROR);
                return false;
            }

            try
            {
                File.Copy(ManifestProjectFilePath, newProjectFileName);
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return false;
            }

            SetPathAndManifestOfZipfile(newProjectFileName, false);
            Manifest.BSDL.ForEach(bsdl => DeleteContentFromZip(bsdl.BSDLFileName));
            Manifest.BSDL.Clear();

            try
            {
                if (!DeleteAllSvfEntries())
                {
                    logger.LogMessage("Error at deleting all existing SVF files from project", LogCategory.ERROR);
                }
            }
            catch (CustomCollectionException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }

            UpdateManifestInZipfile(Manifest);
            logger.LogMessage(
                "Created successfully a new project file and opened it: " + ManifestProjectFilePath, LogCategory.INFO);
            return true;
        }

        private string GetNewFileName()
        {
            string value = ManifestProjectFilePath.Replace(SVFPROJ, string.Empty);
            if (Manifest != null && Manifest.BSDL != null & Manifest.BSDL.Count > 0)
            {
                foreach (var bsdl in Manifest.BSDL)
                {
                    value = value.Replace("_" + bsdl.JTAGName.Replace(":", "_"), string.Empty);
                }
            }

            return value;
        }

        /// <summary>
        /// DeleteAllSvfEntries
        /// </summary>
        /// <returns>true if everything was ok, otherwise false</returns>
        /// <exception cref="CustomCollectionException">exception</exception>
        private bool DeleteAllSvfEntries()
        {
            try
            {
                using ZipFile archive = ZipFile.Read(ManifestProjectFilePath);
                if (ZipFile.CheckZipPassword(ManifestProjectFilePath, PASSWORD))
                {
                    archive.Password = PASSWORD;
                }
                else
                {
                    logger.LogMessage("Used other password protected projectfile!", LogCategory.ERROR);
                    return false;
                }

                if (!archive.EntryFileNames.Contains(Manifest.SVFDEF.Replace("\\", "/")))
                {
                    return true;
                }

                List<ZipEntry> entriesToDelete = new List<ZipEntry>(archive.Entries.Where(entry =>
                    !entry.FileName.Replace("/", "\\", StringComparison.InvariantCulture).Equals(Manifest.SVFDEF)
                    && entry.FileName.Replace("/", "\\", StringComparison.InvariantCulture).StartsWith(
                        Manifest.SVFDEF, StringComparison.InvariantCulture)));
                archive.RemoveEntries(entriesToDelete);
                archive.Save();
                return true;
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }
    }
}
