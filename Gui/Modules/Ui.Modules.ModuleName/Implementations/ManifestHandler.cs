using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Interfaces.Gui;
using Newtonsoft.Json;
using SVFHelper.Interfaces;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class ManifestHandler : IManifestHandler, IProjectHandler
    {
        private static readonly string TEMPPATH = Directory.GetCurrentDirectory() + "\\tempPathForManifestHandling\\";
        private readonly ILogger logger;
        private readonly IDialogSelector dialogSelector;
        private string manifestProjectFilePath;
        private Manifest manifest;

        public ManifestHandler(ILogger logger, IDialogSelector dialogSelector)
        {
            this.logger = logger;
            this.dialogSelector = dialogSelector;
        }

        public void SetPathAndManifestOfZipfile(string path, bool newCreation)
        {
            if (!newCreation && !File.Exists(path))
            {
                logger.LogMessage("No valid project file selected!", LogCategory.ERROR);
                return;
            }

            manifestProjectFilePath = path;
            if (!newCreation)
            {
                manifest = GetObjectFromZipArchive<Manifest>(Manifest.MANIFESTNAME);
                if (manifest == null)
                {
                    manifest = new Manifest();
                    logger.LogMessage("Error finding the manifest file within the project file!", LogCategory.ERROR);
                    if (!UpdateManifestInZipfile(manifest))
                    {
                        logger.LogMessage("Error updating the manifest file in the existing project!", LogCategory.ERROR);
                        manifest = null;
                        manifestProjectFilePath = string.Empty;
                        return;
                    }
                }
            }
            else
            {
                manifest = new Manifest();
                string manifestString = GetObjecttAsString(manifest);
                if (string.IsNullOrEmpty(manifestString))
                {
                    return;
                }

                try
                {
                    if (Directory.Exists(TEMPPATH))
                    {
                        try
                        {
                            DeleteDirectory(TEMPPATH);
                        }
                        catch (CustomCollectionException e)
                        {
                            logger.LogMessage(e.Message, LogCategory.ERROR);
                            return;
                        }
                    }

                    CreateDirectory(TEMPPATH);
                    CreateNewFileAndWriteDataInto(TEMPPATH + Manifest.MANIFESTNAME, manifestString);
                    CreateZipFile(TEMPPATH, manifestProjectFilePath);
                }
                catch (CustomCollectionException e)
                {
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                }

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

        public string GetProjectSelectionPath()
        {
            dialogSelector.OpenGenericDialog(DialogType.OPENFILE, TextRessources.SelectProjectFileForOpening, "No path selected for project import!", out string path);
            return path;
        }

        public string GetBomData()
        {
            return GetBomFile();
        }

        public BomSettings GetBomSettings()
        {
            return GetBomSettingsFile();
        }

        public async Task<string> GetOdbProjectFolder()
        {
            return await GetOdbProjectPath().ConfigureAwait(false);
        }

        public void ExportJsonProjectFileContent(byte[] data, string fileName)
        {
            if (UpdateJsonProject(data, fileName[(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)..]))
            {
                manifest.SetJsonProject(fileName[(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)..]);
                UpdateManifestInZipfile(manifest);
                Application.Current.Dispatcher.Invoke(() => logger.LogMessage("Save of JSON project file in project file successful", LogCategory.INFO));
            }
        }

        public byte[] GetJsonProjectContent()
        {
            return GetJsonProjectFile();
        }

        public void ExportSettingsFileContent(byte[] data, string fileName)
        {
            if (UpdateSettingsFile(data, fileName[(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)..]))
            {
                manifest.SetSettingsFile(fileName[(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)..]);
                UpdateManifestInZipfile(manifest);
                logger.LogMessage("Successfully saved settings into project file", LogCategory.INFO);
            }
        }

        public bool IsManifestHandlingActive()
        {
            return !string.IsNullOrEmpty(manifestProjectFilePath) && manifest != null && File.Exists(manifestProjectFilePath);
        }

        public void ResetValues()
        {
            manifestProjectFilePath = string.Empty;
            manifest = null;
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

        public Manifest GetActuallyLoadedManifest()
        {
            return manifest;
        }

        public string GetBomFile()
        {
            if (!IsManifestHandlingActive())
            {
                return string.Empty;
            }

            return GetObjectFromZipArchive<string>(manifest.BomFile);
        }

        public BomSettings GetBomSettingsFile()
        {
            if (!IsManifestHandlingActive())
            {
                return null;
            }

            return GetObjectFromZipArchive<BomSettings>(manifest.BomSettingsFile);
        }

        public byte[] GetJsonProjectFile()
        {
            if (!IsManifestHandlingActive())
            {
                return null;
            }

            string result = GetObjectFromZipArchive<string>(manifest.JsonProject);
            if (!string.IsNullOrEmpty(result))
            {
                return Encoding.ASCII.GetBytes(result);
            }

            return null;
        }

        public async Task<string> GetOdbProjectPath()
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

        public byte[] GetSettingsFile()
        {
            if (!IsManifestHandlingActive())
            {
                return null;
            }

            byte[] result = GetObjectFromZipArchive<byte[]>(manifest.SettingsFile);
            return result;
        }

        public bool UpdateBomFile(byte[] fileContent, string newFileName)
        {
            bool result = HandleSavingOfZipEntry(manifest.BomFile, fileContent, Manifest.BOMLOCDEF + newFileName);
            if (result)
            {
                manifest.SetBomFile(newFileName);
                UpdateManifestInZipfile(manifest);
            }

            return result;
        }

        public bool UpdateBomSettingsFile(BomSettings settings, string newFileName)
        {
            bool result = HandleSavingOfZipEntry(manifest.BomSettingsFile, Encoding.ASCII.GetBytes(GetObjecttAsString(settings)), Manifest.BOMLOCDEF + newFileName);
            if (result)
            {
                manifest.SetBomSettingsFile(newFileName);
                UpdateManifestInZipfile(manifest);
            }

            return result;
        }

        public void ExportSvfFiles(List<ISvfData> data)
        {
            if (!IsManifestHandlingActive())
            {
                logger.LogMessage("Error using project mode for exporting the SVF files!", LogCategory.ERROR);
                return;
            }

            try
            {
                DeleteSvfEntries(data);
            }
            catch (CustomCollectionException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }

            try
            {
                AddSvfEntries(data);
            }
            catch (CustomCollectionException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
            }
        }

        public bool UpdateJsonProject(byte[] fileContent, string newFileName = "")
        {
            string fileToUse = newFileName;
            if (string.IsNullOrEmpty(fileToUse))
            {
                fileToUse = manifest.JsonProject[(manifest.JsonProject.LastIndexOf("\\", StringComparison.Ordinal) + 1)..];
            }

            return HandleSavingOfZipEntry(manifest.JsonProject, fileContent, Manifest.JSONLOCDEF + fileToUse);
        }

        public bool UpdateSettingsFile(byte[] fileContent, string newFileName = "")
        {
            string fileToUse = newFileName;
            if (string.IsNullOrEmpty(fileToUse))
            {
                fileToUse = manifest.SettingsFile[(manifest.SettingsFile.LastIndexOf("\\", StringComparison.Ordinal) + 1)..];
            }

            return HandleSavingOfZipEntry(manifest.SettingsFile, fileContent, Manifest.SETTINGSLOCDEF + fileToUse);
        }

        public bool UpdateManifestInZipfile(Manifest manifestValue)
        {
            manifest = manifestValue;
            byte[] manifestContent = Encoding.ASCII.GetBytes(GetObjecttAsString(manifest));
            if (manifestContent == null || manifestContent.Length == 0)
            {
                return false;
            }

            return HandleSavingOfZipEntry(Manifest.MANIFESTNAME, manifestContent, Manifest.MANIFESTNAME);
        }

        public async Task<bool> UpdateOdbProject(string pathToFolder)
        {
            return await Task.Run<bool>(() =>
            {
                try
                {
                    DeleteContentFromZipWithAllSubfolders();
                    return UpdateObdProjectInZip(pathToFolder);
                }
                catch (CustomCollectionException e)
                {
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                    return false;
                }
            }).ConfigureAwait(false);
        }

        public byte[] GetSettingsFileContent()
        {
            return GetSettingsFile();
        }

        /// <summary>
        /// GetArchiveForPath
        /// </summary>
        /// <param name="path">the path</param>
        /// <returns>tthe archive</returns>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        private static ZipArchive GetArchiveForPath(string path)
        {
            try
            {
                return ZipFile.OpenRead(path);
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
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
                ZipFile.CreateFromDirectory(path, target);
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

                    if (!Directory.Exists(TEMPPATH + manifest.OdbProject))
                    {
                        Directory.CreateDirectory(TEMPPATH + manifest.OdbProject);
                    }

                    using ZipArchive archive = ZipFile.OpenRead(manifestProjectFilePath);
                    bool found = false;
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.Name.Contains(manifest.OdbProject) || entry.FullName.Replace("/", "\\").Contains(manifest.OdbProject))
                        {
                            found = true;
                            if (!entry.FullName.Equals(manifest.OdbProject))
                            {
                                string path = Path.GetDirectoryName(TEMPPATH + entry.FullName);
                                if (!Directory.Exists(path))
                                {
                                    Directory.CreateDirectory(path);
                                }

                                entry.ExtractToFile(TEMPPATH + entry.FullName);
                            }
                        }
                    }

                    if (found)
                    {
                        return TEMPPATH + manifest.OdbProject;
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

        private bool HandleSavingOfZipEntry(string filename, byte[] fileContent, string newFileName)
        {
            if (!IsManifestHandlingActive())
            {
                return false;
            }

            try
            {
                DeleteContentFromZip(filename);
                CreateZipEntry(newFileName, fileContent);
                return true;
            }
            catch (CustomCollectionException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return false;
            }
        }

        /// <summary>
        /// CreateZipEntry
        /// </summary>
        /// <param name="fileName">fileName</param>
        /// <param name="fileContent">fileContent</param>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        private void CreateZipEntry(string fileName, byte[] fileContent)
        {
            try
            {
                using ZipArchive archive = ZipFile.Open(manifestProjectFilePath, ZipArchiveMode.Update);
                ZipArchiveEntry entry = archive.CreateEntry(fileName);
                entry.Open().Write(fileContent);
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        /// <summary>
        /// DeleteContentFromZip
        /// </summary>
        /// <param name="fileName">the filename</param>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        private void DeleteContentFromZip(string fileName)
        {
            try
            {
                using ZipArchive archive = ZipFile.Open(manifestProjectFilePath, ZipArchiveMode.Update);
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.Equals(fileName) || entry.FullName.Replace("/", "\\").Equals(fileName))
                    {
                        entry.Delete();
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        private string GetObjecttAsString(object obj)
        {
            try
            {
                return JsonConvert.SerializeObject(obj, Formatting.Indented);
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

            return string.Empty;
        }

        /// <summary>
        /// DeleteContentFromZipWithAllSubfolders
        /// </summary>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        private void DeleteContentFromZipWithAllSubfolders()
        {
            try
            {
                using ZipArchive archive = ZipFile.Open(manifestProjectFilePath, ZipArchiveMode.Update);
                List<ZipArchiveEntry> entriesToDelete = new List<ZipArchiveEntry>();
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.StartsWith(manifest.OdbProject, StringComparison.Ordinal) ||
                        entry.FullName.Replace("/", "\\").StartsWith(manifest.OdbProject, StringComparison.Ordinal))
                    {
                        entriesToDelete.Add(entry);
                    }
                }

                foreach (var entryToDelete in entriesToDelete)
                {
                    entryToDelete.Delete();
                }
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        /// <summary>
        /// UpdateObdProjectInZip
        /// </summary>
        /// <param name="pathToFolder">pathToFolder</param>
        /// <returns>bool if true otherwise false</returns>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        private bool UpdateObdProjectInZip(string pathToFolder)
        {
            if (!pathToFolder.EndsWith("\\", StringComparison.Ordinal))
            {
                pathToFolder += "\\";
            }

            try
            {
                using (ZipArchive archive = ZipFile.Open(manifestProjectFilePath, ZipArchiveMode.Update))
                {
                    archive.CreateEntry(manifest.OdbProject);
                    foreach (var file in Directory.GetFiles(pathToFolder, "*.*", SearchOption.AllDirectories))
                    {
                        archive.CreateEntryFromFile(file, manifest.OdbProject + file.Replace(pathToFolder, string.Empty));
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        private T GetObjectFromZipArchive<T>(string fileName)
        {
            try
            {
                using ZipArchive archive = GetArchiveForPath(manifestProjectFilePath);
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.Equals(fileName) || entry.FullName.Replace("/", "\\").Equals(fileName))
                    {
                        return GetObjectFromZipEntry<T>(entry.Open(), entry.Length);
                    }
                }

                return default;
            }
            catch (CustomCollectionException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return default;
            }
        }

        private T GetObjectFromZipEntry<T>(Stream stream, long length)
        {
            if (typeof(T) == typeof(byte[]))
            {
                byte[] result = new byte[length];
                stream.Read(result);
                return (T)(object)result;
            }

            string bodyString = string.Empty;
            try
            {
                using StreamReader bodyReader = new StreamReader(stream);
                bodyString = bodyReader.ReadToEnd();
            }
            catch (ArgumentException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return default;
            }
            catch (IOException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return default;
            }
            catch (OutOfMemoryException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return default;
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)bodyString;
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(bodyString);
            }
            catch (JsonException e)
            {
                logger.LogMessage("Error parsing settings file: " + e.Message, LogCategory.ERROR);
                return default;
            }
        }

        /// <summary>
        /// AddSvfEntries
        /// </summary>
        /// <param name="data">data</param>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        private void AddSvfEntries(List<ISvfData> data)
        {
            List<string> notFoundPins = new List<string>();
            int count = 0;
            try
            {
                using (ZipArchive archive = ZipFile.Open(manifestProjectFilePath, ZipArchiveMode.Update))
                {
                    if (archive.GetEntry(manifest.Svf) == null)
                    {
                        archive.CreateEntry(manifest.Svf);
                    }

                    foreach (var svf in data)
                    {
                        if (!string.IsNullOrEmpty(svf.SvfRequestContent))
                        {
                            ZipArchiveEntry entry = archive.CreateEntry(manifest.Svf + svf.GetCompleteName());
                            entry.Open().Write(ASCIIEncoding.ASCII.GetBytes(svf.GetCompleteContentAsString()));
                            count++;
                        }
                        else if (!notFoundPins.Contains(svf.PinName))
                        {
                            notFoundPins.Add(svf.PinName);
                        }
                    }
                }

                string notFound = string.Empty;
                foreach (var pin in notFoundPins)
                {
                    notFound += pin + ", ";
                }

                if (!string.IsNullOrEmpty(notFound))
                {
                    logger.LogMessage("The following " + notFoundPins.Count + " PINS were not found for JTAG: " + data[0].JtagName + ": " + notFound[0..^2], LogCategory.WARNING);
                }

                logger.LogMessage("Successfully exported " + count + " SVF files into project", LogCategory.INFO);
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        /// <summary>
        /// DeleteSvfEntries
        /// </summary>
        /// <param name="data">data</param>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        private void DeleteSvfEntries(List<ISvfData> data)
        {
            try
            {
                using ZipArchive archive = ZipFile.Open(manifestProjectFilePath, ZipArchiveMode.Update);
                if (archive.GetEntry(manifest.Svf) == null)
                {
                    return;
                }

                List<ZipArchiveEntry> entriesToDelete = new List<ZipArchiveEntry>();
                foreach (var entry in archive.Entries)
                {
                    if (entry.Name.Replace("/", "\\", StringComparison.InvariantCulture).StartsWith(manifest.Svf, StringComparison.InvariantCulture) ||
                        entry.FullName.Replace("/", "\\").StartsWith(manifest.Svf, StringComparison.InvariantCulture))
                    {
                        foreach (var svf in data)
                        {
                            if (entry.Name.Replace("-", "_").Equals(svf.GetCompleteName()) || entry.FullName.Replace("-", "_").EndsWith(svf.GetCompleteName(), StringComparison.InvariantCulture))
                            {
                                entriesToDelete.Add(entry);
                                break;
                            }
                        }
                    }
                }

                foreach (var entry in entriesToDelete)
                {
                    entry.Delete();
                }
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }
    }
}
