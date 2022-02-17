using Ionic.Zip;
using Newtonsoft.Json;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.SvfHelper;
using ProMik.SmartIct.Services.ManifestHandler.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.Svf.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace ProMik.SmartIct.Services.ManifestHandler.Implementations
{
    public class ManifestActionsHandler : IManifestHandler
    {
        protected const string PASSWORD = "ProMik@100!";
        protected const string SettingJsonFileName = "settings.json";
        protected readonly ILogger logger;

        public ManifestActionsHandler(ILogger logger)
        {
            this.logger = logger;
        }

        public Manifest Manifest { get; protected set; }

        public string ManifestProjectFilePath { get; private set; }

        public bool SaveBsdlContentIntoProjectFile(BsdlContainer bsdl, byte[] data)
        {
            try
            {
                HandleSavingOfZipEntry(bsdl.BSDLFileName, data, bsdl.BSDLFileName);
                Manifest.BSDL.Add(bsdl);
                if (!UpdateManifestInZipfile(Manifest))
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return false;
            }
        }

        public virtual Stream GetBsdlContenAsStream(out string fileName, string jtagDevice = "")
        {
            string pathToUse = string.Empty;
            if (!string.IsNullOrEmpty(jtagDevice))
            {
                pathToUse = GetBsdlPathToUse(jtagDevice);
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

            UpdateSvfPaths(data);
        }

        /// <summary>
        /// GetArchiveForPath
        /// </summary>
        /// <param name="path">the path</param>
        /// <returns>tthe archive</returns>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        public ZipFile GetArchiveForPath(string path)
        {
            try
            {
                ZipFile file = ZipFile.Read(path);
                if (ZipFile.CheckZipPassword(path, PASSWORD))
                {
                    file.Password = PASSWORD;
                }
                else
                {
                    logger.LogMessage("Used other password protected projectfile!", LogCategory.ERROR);
                    return null;
                }

                return file;
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        public virtual void ExportSettingsFileContent(byte[] data, string fileName, bool useOnlyJsonSettingsFileAsByteContent = false)
        {
            UpdateSettingsFile(data, fileName[(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)..], useOnlyJsonSettingsFileAsByteContent);
            Manifest.SetSettingsFile(fileName[(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)..]);
            UpdateManifestInZipfile(Manifest);
            logger.LogMessage("Successfully saved settings into project file", LogCategory.INFO);
        }

        public bool UpdateSettingsFile(byte[] fileContent, string newFileName = "", bool useOnlyJsonSettingsFileAsByteContent = false)
        {
            string fileToUse = newFileName;
            if (string.IsNullOrEmpty(fileToUse))
            {
                fileToUse = Manifest.SettingsFile[(Manifest.SettingsFile.LastIndexOf("/", StringComparison.Ordinal) + 1)..];
            }

            return HandleSavingOfZipEntry(Manifest.SettingsFile, fileContent, Manifest.SETTINGSLOCDEF + fileToUse); ;
        }

        public string GetBomData()
        {
            if (!IsManifestHandlingActive())
            {
                return string.Empty;
            }

            return GetObjectFromZipArchive<string>(Manifest.BomFile);
        }

        public string GetObjecttAsJsonString(object obj)
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

        public BomSettings GetBomSettings()
        {
            if (!IsManifestHandlingActive())
            {
                return null;
            }

            return GetObjectFromZipArchive<BomSettings>(Manifest.BomSettingsFile);
        }

        public byte[] GetSettingsFileContent(bool useJsonSettingsFile = false)
        {
            if (!IsManifestHandlingActive())
            {
                return null;
            }

            byte[] result = GetObjectFromZipArchive<byte[]>(!useJsonSettingsFile ? Manifest.SettingsFile : Manifest.SETTINGSLOCDEF + SettingJsonFileName);
            return result;
        }

        public byte[] GetJsonProjectContent()
        {
            if (!IsManifestHandlingActive())
            {
                return null;
            }

            string result = GetObjectFromZipArchive<string>(Manifest.JsonProject);
            if (!string.IsNullOrEmpty(result))
            {
                return Encoding.ASCII.GetBytes(result);
            }

            return null;
        }

        public T GetObjectFromZipArchive<T>(string fileName)
        {
            try
            {
                using ZipFile archive = GetArchiveForPath(ManifestProjectFilePath);
                if (archive != null)
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (entry.FileName.Equals(fileName) || entry.FileName.Replace("/", "\\").Equals(fileName))
                        {
                            T result = GetObjectFromZipEntry<T>(entry.OpenReader(), entry.UncompressedSize);
                            archive.Dispose();
                            return result;
                        }
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

        public void SetPathAndManifestOfZipfile(string path, bool newCreation)
        {
            if (!newCreation && !File.Exists(path))
            {
                logger.LogMessage("No valid project file selected!", LogCategory.ERROR);
                return;
            }

            ManifestProjectFilePath = path;
            if (!newCreation)
            {
                Manifest = GetObjectFromZipArchive<Manifest>(Manifest.MANIFESTNAME);
                if (Manifest == null)
                {
                    Manifest = new Manifest();
                    logger.LogMessage("Error finding the manifest file within the project file!", LogCategory.ERROR);
                    if (!UpdateManifestInZipfile(Manifest))
                    {
                        logger.LogMessage("Error updating the manifest file in the existing project!", LogCategory.ERROR);
                        Manifest = null;
                        ManifestProjectFilePath = string.Empty;
                        return;
                    }
                }
                else if (ZipFile.CheckZipPassword(ManifestProjectFilePath, "Nothing"))
                {
                    logger.LogMessage(
                        "Used not password protected project file: "
                        + ManifestProjectFilePath,
                        LogCategory.WARNING);
                }
                else if (!ZipFile.CheckZipPassword(ManifestProjectFilePath, PASSWORD))
                {
                    logger.LogMessage("Used other password protected projectfile!", LogCategory.ERROR);
                    return;
                }
            }
            else
            {
                Manifest = new Manifest();
                try
                {
                    CreateZipFileWithManifest(Manifest, ManifestProjectFilePath);
                }
                catch (CustomCollectionException e)
                {
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                }
            }
        }

        /// <summary>
        /// DeleteContentFromZipWithAllSubfolders
        /// </summary>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        public void DeleteContentFromZipWithAllSubfolders(string nameToUse)
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
                    return;
                }

                List<ZipEntry> entriesToDelete = new List<ZipEntry>(archive.Entries.Where(
                    entry => entry.FileName.StartsWith(nameToUse, StringComparison.Ordinal)
                    || entry.FileName.Replace("/", "\\").StartsWith(nameToUse, StringComparison.Ordinal)));
                archive.RemoveEntries(entriesToDelete);
                archive.Save();
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        public virtual void ResetValues(bool skipFolderDeletion = false)
        {
            ManifestProjectFilePath = string.Empty;
            Manifest = null;
        }

        public bool UpdateBomFile(byte[] fileContent, string newFileName)
        {
            bool result = HandleSavingOfZipEntry(Manifest.BomFile, fileContent, Manifest.BOMLOCDEF + newFileName);
            if (result)
            {
                Manifest.SetBomFile(newFileName);
                UpdateManifestInZipfile(Manifest);
            }

            return result;
        }

        public bool UpdateBomSettingsFile(BomSettings settings, string newFileName)
        {
            bool result = HandleSavingOfZipEntry(
                Manifest.BomSettingsFile,
                Encoding.ASCII.GetBytes(GetObjecttAsJsonString(settings)),
                Manifest.BOMLOCDEF + newFileName);
            if (result)
            {
                Manifest.SetBomSettingsFile(newFileName);
                UpdateManifestInZipfile(Manifest);
            }

            return result;
        }

        public async Task<bool> UpdateOdbProject(string pathToFolder)
        {
            return await Task.Run(() =>
            {
                try
                {
                    DeleteContentFromZipWithAllSubfolders(Manifest.OdbProject);
                    return UpdateObdProjectInZip(pathToFolder);
                }
                catch (CustomCollectionException e)
                {
                    logger.LogMessage(e.Message, LogCategory.ERROR);
                    return false;
                }
            }).ConfigureAwait(false);
        }

        public void ExportJsonProjectFileContent(byte[] data, string fileName)
        {
            if (UpdateJsonProject(data, fileName[(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)..]))
            {
                Manifest.SetJsonProject(fileName[(fileName.LastIndexOf("\\", StringComparison.Ordinal) + 1)..]);
                UpdateManifestInZipfile(Manifest);
                logger.LogMessage("Save of JSON project file in project file successful", LogCategory.INFO);
            }
        }

        public bool UpdateManifestInZipfile(Manifest manifestValue)
        {
            Manifest = manifestValue;
            byte[] manifestContent = Encoding.ASCII.GetBytes(GetObjecttAsJsonString(Manifest));
            if (manifestContent == null || manifestContent.Length == 0)
            {
                return false;
            }

            return HandleSavingOfZipEntry(Manifest.MANIFESTNAME, manifestContent, Manifest.MANIFESTNAME);
        }

        public bool UpdateJsonProject(byte[] fileContent, string newFileName = "")
        {
            string fileToUse = newFileName;
            if (string.IsNullOrEmpty(fileToUse))
            {
                fileToUse = Manifest.JsonProject[(Manifest.JsonProject.LastIndexOf("/", StringComparison.Ordinal) + 1)..];
            }

            return HandleSavingOfZipEntry(Manifest.JsonProject, fileContent, Manifest.JSONLOCDEF + fileToUse);
        }

        public bool IsManifestHandlingActive()
        {
            return !string.IsNullOrEmpty(ManifestProjectFilePath) && Manifest != null && File.Exists(ManifestProjectFilePath);
        }

        protected static string GetDestinationForSvf(ISvfData svf)
        {
            return Path.Combine(Manifest.SVFDEF, svf.JtagName.Replace(":", "_"), GetShortenNeighbourPath(svf.SvfWriter)).Replace("\\", "/");
        }

        protected static string GetShortenNeighbourPath(ISvfWriter svfWriter)
        {
            if (svfWriter.MultiPinType != MultiPinType.None)
            {
                return svfWriter.DestinationPath.Replace(SvfConstant.NEIGHBOURS, string.Empty);
            }

            return svfWriter.DestinationPath;
        }

        protected void UpdateSvfPaths(List<ISvfData> data)
        {
            foreach (var bsdlContent in Manifest.BSDL)
            {
                List<ISvfData> sortedDataByJtag = data.Where(svf => svf.JtagName.Equals(bsdlContent.JTAGName)).ToList();
                if (sortedDataByJtag.Count == 0)
                {
                    continue;
                }

                Dictionary<BoundaryScanTestType, ISvfData> testTypes = new Dictionary<BoundaryScanTestType, ISvfData>();
                foreach (var svf in sortedDataByJtag)
                {
                    if (!testTypes.ContainsKey(svf.TestType))
                    {
                        testTypes.Add(svf.TestType, svf);
                    }
                }

                bsdlContent.SvfPath = GetSvfPaths(testTypes);
            }

            UpdateManifestInZipfile(Manifest);
        }

        protected static SvfPath GetSvfPaths(Dictionary<BoundaryScanTestType, ISvfData> testTypes)
        {
            return new SvfPath()
            {
                DirectGnd = GetSvfPath(testTypes, BoundaryScanTestType.DirectGnd),
                DirectPower = GetSvfPath(testTypes, BoundaryScanTestType.DirectPower),
                PullDowns = GetSvfPath(testTypes, BoundaryScanTestType.Pulldown),
                PullUps = GetSvfPath(testTypes, BoundaryScanTestType.Pullup),
                PullUpsAndDowns = GetSvfPath(testTypes, BoundaryScanTestType.PullupPulldown),
                UnknownControls = GetSvfPath(testTypes, BoundaryScanTestType.UnknownControl),
                UnknownInputs = GetSvfPath(testTypes, BoundaryScanTestType.UnknownInput),
            };
        }

        protected static string GetSvfPath(
            Dictionary<BoundaryScanTestType, ISvfData> testTypes,
            BoundaryScanTestType testTypeToFind)
        {
            if (testTypes.ContainsKey(testTypeToFind))
            {
                return GetDestinationForSvf(testTypes[testTypeToFind]);
            }

            return string.Empty;
        }

        /// <summary>
        /// DeleteSvfEntries
        /// </summary>
        /// <param name="data">data</param>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        protected void DeleteSvfEntries(List<ISvfData> data)
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
                    return;
                }

                if (!archive.EntryFileNames.Contains(Manifest.SVFDEF.Replace("\\", "/")))
                {
                    return;
                }

                List<ZipEntry> entriesToDelete = new List<ZipEntry>();
                foreach (var entry in archive.Entries.Where(
                    entr => entr.FileName.Replace("/", "\\", StringComparison.InvariantCulture)
                    .StartsWith(Manifest.SVFDEF, StringComparison.InvariantCulture)))
                {
                    foreach (var svf in data)
                    {
                        if (entry.FileName.Replace("-", "_")
                            .Equals(svf.GetCompleteName()) || entry.FileName.Replace("-", "_")
                            .EndsWith(svf.GetCompleteName(), StringComparison.InvariantCulture))
                        {
                            entriesToDelete.Add(entry);
                            break;
                        }
                    }
                }

                archive.RemoveEntries(entriesToDelete);
                archive.Save();
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        /// <summary>
        /// AddSvfEntries
        /// </summary>
        /// <param name="data">data</param>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        protected void AddSvfEntries(List<ISvfData> data)
        {
            int count = 0;
            try
            {
                using (ZipFile archive = ZipFile.Read(ManifestProjectFilePath))
                {
                    if (ZipFile.CheckZipPassword(ManifestProjectFilePath, PASSWORD))
                    {
                        archive.Password = PASSWORD;
                    }
                    else
                    {
                        logger.LogMessage("Used other password protected projectfile!", LogCategory.ERROR);
                        return;
                    }

                    if (!archive.EntryFileNames.Contains(Manifest.SVFDEF.Replace("\\", "/")))
                    {
                        archive.AddDirectoryByName(Manifest.SVFDEF);
                    }

                    foreach (var svf in data)
                    {
                        string destinationPath = GetDestinationForSvf(svf).Replace("\\", "/") + "/";
                        if (!archive.EntryFileNames.Contains(destinationPath))
                        {
                            archive.AddDirectoryByName(destinationPath);
                        }

                        ZipEntry entry = archive.AddEntry(
                            Path.Combine(destinationPath, svf.GetCompleteName())
                            .Replace("\\", "/"),
                            Encoding.ASCII.GetBytes(svf.GetCompleteContentAsString()));
                        count++;
                    }

                    archive.Save();
                }

                logger.LogMessage("Successfully exported " + count + " SVF files into project", LogCategory.INFO);
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
        protected bool UpdateObdProjectInZip(string pathToFolder)
        {
            if (!pathToFolder.EndsWith("\\", StringComparison.Ordinal))
            {
                pathToFolder += "\\";
            }

            try
            {
                using (ZipFile archive = ZipFile.Read(ManifestProjectFilePath))
                {
                    if (ZipFile.CheckZipPassword(ManifestProjectFilePath, PASSWORD))
                    {
                        archive.Password = PASSWORD;
                    }
                    else
                    {
                        logger.LogMessage("Used other password protected projectfile!", LogCategory.ERROR);
                        return false;
                    }

                    archive.AddDirectory(pathToFolder, Manifest.OdbProject);
                    archive.Save();
                }

                return true;
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        protected Stream GetZipStream(string value)
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
                    return null;
                }

                ZipEntry entry = archive.Entries.FirstOrDefault(x => x.FileName.Equals(value));
                if (entry == null)
                {
                    var entryInner = archive.Entries.FirstOrDefault(entryIn => entryIn.FileName.Equals(value.Replace("\\", "/"))
                        || entryIn.FileName.Equals(value.Replace("\\", "/")));
                    if (entryInner != null)
                    {
                        entry = entryInner;
                    }
                }

                if (entry != null)
                {
                    MemoryStream mem = new MemoryStream((int)entry.UncompressedSize);
                    entry.OpenReader().CopyTo(mem);
                    mem.Seek(0, SeekOrigin.Begin);
                    mem.Position = 0;
                    return mem;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return null;
            }
        }

        protected string GetBsdlPathToUse(string jtagDevice)
        {
            if (!IsManifestHandlingActive())
            {
                return string.Empty;
            }

            if (Manifest.BSDL == null || Manifest.BSDL.Count == 0)
            {
                return string.Empty;
            }

            return Manifest.BSDL.FirstOrDefault(bs => bs.JTAGName.Equals(jtagDevice))?.BSDLFileName ?? string.Empty;
        }

        protected void CreateZipFileWithManifest(Manifest manifest, string manifestProjectFilePath)
        {
            try
            {
                if (File.Exists(manifestProjectFilePath))
                {
                    File.Delete(manifestProjectFilePath);
                }

                using ZipFile zip = new ZipFile();
                zip.Password = PASSWORD;
                var data = GetObjecttAsJsonString(manifest);
                zip.AddEntry(Manifest.MANIFESTNAME, data);
                zip.Save(manifestProjectFilePath);
            }
            catch (Exception e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                throw new CustomCollectionException(e.Message);
            }
        }

        protected T GetObjectFromZipEntry<T>(Stream stream, long length)
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

        protected bool HandleSavingOfZipEntry(string filename, byte[] fileContent, string newFileName)
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
        /// DeleteContentFromZip
        /// </summary>
        /// <param name="fileName">the filename</param>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        protected void DeleteContentFromZip(string fileName)
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
                    return;
                }

                ZipEntry entryToDelete = archive.Entries.FirstOrDefault(
                    entry => entry.FileName.Equals(fileName) || entry.FileName.Replace("/", "\\").Equals(fileName));
                if (entryToDelete != null)
                {
                    archive.RemoveEntry(entryToDelete);
                    archive.Save();
                }
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }

        /// <summary>
        /// CreateZipEntry
        /// </summary>
        /// <param name="fileName">fileName</param>
        /// <param name="fileContent">fileContent</param>
        /// <exception cref="Ui.Modules.ModuleName.Helper.CustomCollectionException">Ignore.</exception>
        protected void CreateZipEntry(string fileName, byte[] fileContent)
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
                    return;
                }

                MemoryStream memstream = new MemoryStream(fileContent);
                byte[] bytes = memstream.ToArray();
                archive.AddEntry(fileName, bytes);
                archive.Save();
                memstream.Close();
            }
            catch (Exception e)
            {
                throw new CustomCollectionException(e.Message);
            }
        }
    }
}
