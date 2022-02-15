using Ionic.Zip;
using Newtonsoft.Json;
using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.Core.Interfaces.Results;
using ProMik.SmartIct.Console.Contracts.Implementations;
using ProMik.SmartIct.Console.ProjectFileHandler.Interfaces;
using ProMik.Svf.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Services.ManifestHandler.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.SmartIct.Services.ManifestHandler.Implementations;
using ProMik.SmartIct.Console.ProjectFileHandler.Helper;

namespace ProMik.SmartIct.Console.ProjectFileHandler.Implementations
{
    public class ProjectFileHandler : IProjectFileHandler
    {
        private const string Ok = "Ok";
        private const string SettingsJsonFileName = "settings.json";
        private const string JsonProjectFileName = "jsonProject.json";
        private const string BomSettingsFileName = "bom_settings.json";
        private readonly IManifestHandler manifestHandler;
        private readonly ILogger logger = new ConsoleLogger();

        public ProjectFileHandler()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            manifestHandler = new ManifestActionsHandler(logger);
        }

        public async Task<Result> UpdateOdbProjectZipFolder(string svfProjectFilePath, string odbPath)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message += msg + ", ";
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            await manifestHandler.UpdateOdbProject(odbPath).ConfigureAwait(true);
            manifestHandler.ResetValues();
            return new Result(message, success);
        }

        public Task<Result> UpdateJsonProjectFile(string svfProjectFilePath, byte[] jsonData)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message += msg + ", ";
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            manifestHandler.ExportJsonProjectFileContent(jsonData, JsonProjectFileName);
            manifestHandler.ResetValues();
            return Task.FromResult(new Result(message, success));
        }

        public async Task<Result> UpdateSettingsContent(string svfProjectFilePath, SettingsContent settingsContent)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message += msg + ", ";
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            var data = await GetJsonContentAsBytes(settingsContent).ConfigureAwait(true);
            if (!data.Success)
            {
                return new Result(data.Message, false);
            }

            manifestHandler.ExportSettingsFileContent(data.Data, SettingsJsonFileName, true);
            manifestHandler.ResetValues();
            return new Result(message, success);
        }

        public Task<Result> UpdateBomFile(string svfProjectFilePath, byte[] bomContent)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message += msg + ", ";
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            manifestHandler.UpdateBomFile(bomContent, Manifest.BOMFILEDEF.Substring(Manifest.BOMFILEDEF.LastIndexOf("/") + 1));
            manifestHandler.ResetValues();
            return Task.FromResult(new Result(message, success));
        }

        public Task<Result> UpdateBomSettingsContent(string svfProjectFilePath, BomSettings bomSettings)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message += msg + ", ";
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            manifestHandler.UpdateBomSettingsFile(bomSettings, BomSettingsFileName);
            manifestHandler.ResetValues();
            return Task.FromResult(new Result(message, success));
        }

        public async Task<Result> UpdateBsdlContent(string svfProjectFilePath, List<WrappedBsdlContainer> bsdlContent)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message += msg + ", ";
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            await AddBsdlDataToZipFile(bsdlContent).ConfigureAwait(true);
            manifestHandler.ResetValues();
            return new Result(message, success);
        }

        public Task<Result> UpdateManifestContent(string svfProjectFilePath, Manifest manifest)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message += msg + ", ";
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            manifestHandler.UpdateManifestInZipfile(manifest);
            manifestHandler.ResetValues();
            return Task.FromResult(new Result(message, success));
        }

        public Task<Result> UpdateSvfFiles(string svfProjectFilePath, List<ISvfData> svfFiles)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message += msg + ", ";
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            manifestHandler.ExportSvfFiles(svfFiles);
            manifestHandler.ResetValues();
            return Task.FromResult(new Result(message, success));
        }

        public async Task<Result> UpdateSvfFiles(string svfProjectFilePath, string jtagName, SvfPath svfPathsToUse)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message += msg + ", ";
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            var func = await GetFuncForSvfPath().ConfigureAwait(true);
            await func(new List<WrappedBsdlContainer>()
            {
                new SvfPathWrappedBsdlContainer(new BsdlContainer()
                {
                    JTAGName = jtagName,
                },
                string.Empty,
                svfPathsToUse),
            }).ConfigureAwait(true);

            manifestHandler.ResetValues();
            return new Result(message, success);
        }

        public async Task<Result<ProjectFileContent>> CreateProjectFile(
            string odbProjectFolderPath,
            byte[] jsonProject,
            SettingsContent settingsJsonContent,
            string bomFilePath,
            BomSettings bomSettings,
            List<SvfDataWrappedBsdlContainer> bsdl,
            string destinationFileName)
        {
            return await HandleCreationOfProjectFile(
                odbProjectFolderPath,
                jsonProject,
                settingsJsonContent,
                bomFilePath,
                bomSettings,
                destinationFileName,
                bsdl.Cast<WrappedBsdlContainer>().ToList(),
                await GetFuncForSvfFiles().ConfigureAwait(true));
        }

        public async Task<Result<ProjectFileContent>> CreateProjectFile(
            string odbProjectFolderPath,
            byte[] jsonProject,
            SettingsContent settingsJsonContent,
            string bomFilePath,
            BomSettings bomSettings,
            List<SvfPathWrappedBsdlContainer> bsdl,
            string destinationFileName)
        {
            return await HandleCreationOfProjectFile(
                odbProjectFolderPath,
                jsonProject,
                settingsJsonContent,
                bomFilePath,
                bomSettings,
                destinationFileName,
                bsdl.Cast<WrappedBsdlContainer>().ToList(),
                await GetFuncForSvfPath().ConfigureAwait(true));
        }

        public async Task<Result<ProjectFileContent>> CreateProjectFile(string odbProjectFolderPath, byte[] jsonProject, SettingsContent settingsJsonContent, string bomFilePath, BomSettings bomSettings, List<WrappedBsdlContainer> bsdl, string destinationFileName)
        {
            return await HandleCreationOfProjectFile(
                odbProjectFolderPath,
                jsonProject,
                settingsJsonContent,
                bomFilePath,
                bomSettings,
                destinationFileName,
                bsdl,
                await GetFuncEmpty()).ConfigureAwait(true);
        }

        public async Task<Result<ProjectFileContent>> GetProjectFileContent(string svfProjectFilePath)
        {
            string message = string.Empty;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message += msg + ", ";
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            Manifest manifestResult = manifestHandler.Manifest;
            if (manifestResult == null)
            {
                return new Result<ProjectFileContent>(message, false, null);
            }

            Result<Dictionary<string, byte[]>> bsdlFiles = await GetBsdlFilesAsByteArray(manifestResult.BSDL).ConfigureAwait(true);
            var settingsFile = manifestHandler.GetObjectFromZipArchive<byte[]>(manifestResult.SettingsFile);
            var bomFile = manifestHandler.GetObjectFromZipArchive<byte[]>(manifestResult.BomFile);
            var bomSettingsFile = manifestHandler.GetObjectFromZipArchive<byte[]>(manifestResult.BomSettingsFile);
            var jsonProject = manifestHandler.GetObjectFromZipArchive<byte[]>(manifestResult.JsonProject);
            var odbProject = await GetOdbProject(manifestResult.OdbProject).ConfigureAwait(true);
            var settingsContent = manifestHandler.GetObjectFromZipArchive<SettingsContent>(Manifest.SETTINGSLOCDEF + SettingsJsonFileName);
            var bomSettingsContent = manifestHandler.GetObjectFromZipArchive<BomSettings>(manifestResult.BomSettingsFile);
            var bsdlContent = await GetBsdlFiles(svfProjectFilePath).ConfigureAwait(true);
           
            if (message.Length > 2)
            {
                message = message.Substring(0, message.Length - 2);
            }
            else
            {
                message = Ok;
            }

            return new Result<ProjectFileContent>(message, success, new ProjectFileContent(odbProject.Data, jsonProject, settingsFile, bomFile,
                bomSettingsFile, settingsContent, bomSettingsContent, bsdlFiles.Data, bsdlContent.Data, manifestResult));
        }

        public async Task<Result<byte[]>> GetBomSettingsFile(string svfProjectFilePath)
        {
            var result = await GetBomSettingsContent(svfProjectFilePath);
            if (!result.Success)
            {
                return new Result<byte[]>(result.Message, false, null);
            }

            return await GetJsonContentAsBytes(result.Data).ConfigureAwait(true); 
        }

        public Task<Result<Manifest>> GetManifestContent(string svfProjectFilePath)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message = msg;
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            Manifest manifest = manifestHandler.Manifest;
            manifestHandler.ResetValues();
            return Task.FromResult(new Result<Manifest>(message, success, manifestHandler.Manifest));
        }

        public async Task<Result<SettingsContent>> GetSettingsContent(string svfProjectFilePath)
        {
            string message = Ok;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message = msg;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            byte[] data = manifestHandler.GetSettingsFileContent(true);
            manifestHandler.ResetValues();
            if (data != null)
            {
                return await GetObjectFromJsonContent<SettingsContent>(Encoding.ASCII.GetString(data)).ConfigureAwait(true);
            }

            return new Result<SettingsContent>("Error in reading the settings content: " + message, false, null);
        }

        public async Task<Result<byte[]>> GetBomFile(string svfProjectFilePath)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message = msg;
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            string data = await Task.Run<string>(() => manifestHandler.GetBomData()).ConfigureAwait(true);
            manifestHandler.ResetValues();
            return new Result<byte[]>(message, success, Encoding.ASCII.GetBytes(data));
        }

        public async Task<Result<BomSettings>> GetBomSettingsContent(string svfProjectFilePath)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message = msg;
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            BomSettings data = await Task.Run<BomSettings>(() => manifestHandler.GetBomSettings()).ConfigureAwait(true);
            manifestHandler.ResetValues();
            return new Result<BomSettings>(message, success, data);
        }

        public async Task<Result<byte[]>> GetJsonProjectFile(string svfProjectFilePath)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message = msg;
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            byte[] data = await Task.Run<byte[]>(() => manifestHandler.GetJsonProjectContent()).ConfigureAwait(true);
            manifestHandler.ResetValues();
            return new Result<byte[]>(message, success, data);
        }

        public async Task<Result<byte[]>> GetOdbProjectZipFolder(string svfProjectFilePath)
        {
            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            var result = await GetOdbProject(manifestHandler.Manifest.OdbProject).ConfigureAwait(true);
            manifestHandler.ResetValues();
            return result;
        }

        public async Task<Result<byte[]>> GetSettingsFile(string svfProjectFilePath)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message = msg;
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            byte[] data = await Task.Run<byte[]>(() => manifestHandler.GetSettingsFileContent(true)).ConfigureAwait(true);
            manifestHandler.ResetValues();
            return new Result<byte[]>(message, success, data);
        }

        public async Task<Result<Dictionary<string, Stream>>> GetBsdlFiles(string svfProjectFilePath)
        {
            string message = Ok;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message = msg;
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(svfProjectFilePath, false);
            Dictionary<string, Stream> dict = new Dictionary<string, Stream>();
            await Task.Run(() =>
            {
                foreach (var bsdl in manifestHandler.Manifest.BSDL)
                {
                    var result = manifestHandler.GetBsdlContenAsStream(out _, bsdl.JTAGName);
                    if (result != null)
                    {
                        dict.Add(bsdl.JTAGName, result);
                    }
                }
            }).ConfigureAwait(true);

            manifestHandler.ResetValues();
            return new Result<Dictionary<string, Stream>>(message, success, dict);
        }

        private Task<Result<T>> GetObjectFromJsonContent<T>(string jsonContent)
        {
            try
            {
                return Task.FromResult(new Result<T>(Ok, true, JsonConvert.DeserializeObject<T>(jsonContent)));
            }
            catch (Exception e)
            {
                return Task.FromResult(new Result<T>(e.Message, false, (T)(object)null));
            }
        }

        private Task<Func<List<WrappedBsdlContainer>, Task<Result>>> GetFuncForSvfPath()
        {
            Func<List<WrappedBsdlContainer>, Task<Result>> func = async (bsdl) => 
            {
                ZipFile archive = manifestHandler.GetArchiveForPath(manifestHandler.ManifestProjectFilePath);
                if (archive == null)
                {
                    return new Result("Error getting ziparchive: " + manifestHandler.ManifestProjectFilePath, false);
                }

                bool manifestUpdateNecessary = false;
                foreach (var entry in bsdl)
                { 
                    var svfPath = ((SvfPathWrappedBsdlContainer)entry).SvfPathsToUseFromLocalSystem;
                    SvfPath svfPathNew = new SvfPath();
                    await AddFolderByTestTypeIntoZipArchive(archive, svfPath.DirectPower, BoundaryScanTestType.DirectPower, entry.BsdlContainerToUse.JTAGName,
                        svfPathNew)
                        .ConfigureAwait(true);
                    await AddFolderByTestTypeIntoZipArchive(archive, svfPath.DirectGnd, BoundaryScanTestType.DirectGnd, entry.BsdlContainerToUse.JTAGName,
                        svfPathNew)
                        .ConfigureAwait(true);
                    await AddFolderByTestTypeIntoZipArchive(archive, svfPath.PullUps, BoundaryScanTestType.Pullup, entry.BsdlContainerToUse.JTAGName,
                        svfPathNew)
                        .ConfigureAwait(true);
                    await AddFolderByTestTypeIntoZipArchive(archive, svfPath.PullDowns, BoundaryScanTestType.Pulldown, entry.BsdlContainerToUse.JTAGName,
                        svfPathNew)
                        .ConfigureAwait(true);
                    await AddFolderByTestTypeIntoZipArchive(archive, svfPath.PullUpsAndDowns, BoundaryScanTestType.PullupPulldown, entry.BsdlContainerToUse
                        .JTAGName, svfPathNew)
                        .ConfigureAwait(true);
                    await AddFolderByTestTypeIntoZipArchive(archive, svfPath.UnknownControls, BoundaryScanTestType.UnknownControl, entry.BsdlContainerToUse
                        .JTAGName, svfPathNew)
                        .ConfigureAwait(true);
                    await AddFolderByTestTypeIntoZipArchive(archive, svfPath.UnknownInputs, BoundaryScanTestType.UnknownInput, entry.BsdlContainerToUse.JTAGName,
                        svfPathNew)
                        .ConfigureAwait(true);
                    BsdlContainer container = manifestHandler.Manifest.BSDL.FirstOrDefault(x => x.JTAGName.Equals(entry.BsdlContainerToUse.JTAGName));
                    if (container != null)
                    {
                        container.SvfPath = svfPathNew;
                        manifestUpdateNecessary = true;
                    }
                }

                archive.Save();
                if (manifestUpdateNecessary)
                {
                    manifestHandler.UpdateManifestInZipfile(manifestHandler.Manifest);
                }
                
                return new Result(Ok, true);
            };

            return Task.FromResult(func);
        }

        private Task<Func<List<WrappedBsdlContainer>, Task<Result>>> GetFuncForSvfFiles()
        {
            Func<List<WrappedBsdlContainer>, Task<Result>> func = (bsdl) =>
            {
                foreach (var entry in bsdl)
                {
                    manifestHandler.ExportSvfFiles(((SvfDataWrappedBsdlContainer)entry).SvfDataFiles);

                }

                return Task.FromResult(new Result(Ok, true));
            };

            return Task.FromResult(func);
        }

        private async Task<Result<ProjectFileContent>> HandleCreationOfProjectFile(
            string odbProjectFolderPath,
            byte[] jsonProject,
            SettingsContent settingsJsonContent,
            string bomFilePath,
            BomSettings bomSettings,
            string destinationFileName,
            List<WrappedBsdlContainer> bsdl,
            Func<List<WrappedBsdlContainer>, Task<Result>> svfSaveAction)
        {
            if (!destinationFileName.ToLower().EndsWith(".svfproj"))
            {
                destinationFileName += ".svfproj";
            }

            string message = string.Empty;
            bool success = true;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                if (cat == LogCategory.ERROR)
                {
                    message += msg + ", ";
                    success = false;
                }
            });

            manifestHandler.SetPathAndManifestOfZipfile(destinationFileName, true);
            Result<byte[]> settingsContent = await GetJsonContentAsBytes(settingsJsonContent).ConfigureAwait(true);
            if (settingsContent.Success && settingsContent.Data != null)
            {
                manifestHandler.ExportSettingsFileContent(settingsContent.Data, SettingsJsonFileName, true);
            }

            manifestHandler.UpdateBomSettingsFile(bomSettings, BomSettingsFileName);
            manifestHandler.ExportJsonProjectFileContent(jsonProject, JsonProjectFileName);
            string bomFileName = bomFilePath.Substring(bomFilePath.LastIndexOf("\\") + 1);
            var bomFileContent = await GetFileContentAsBytes(bomFilePath).ConfigureAwait(true);
            if (bomFileContent.Success && bomFileContent.Data != null)
            {
                manifestHandler.UpdateBomFile(bomFileContent.Data, bomFileName);
            }

            var odbResult = await manifestHandler.UpdateOdbProject(odbProjectFolderPath).ConfigureAwait(true);
            if (!odbResult)
            {
                success = false;
                message += "Error in trying to add the ODB project into projectfile!, ";
            }

            var svfResult = await AddBsdlDataToZipFile(bsdl)
                .ConfigureAwait(true);
            if (!svfResult.Success)
            {
                success = false;
                message += svfResult.Message + ", ";
            }
            else
            {
                var updateResult = await svfSaveAction(bsdl).ConfigureAwait(true);
                if (!updateResult.Success)
                {
                    success = false;
                    message += updateResult.Message + ", ";
                }
            }

            manifestHandler.ResetValues();
            if (string.IsNullOrEmpty(message))
            {
                message = Ok;
            }
            else
            {
                message = message.Substring(0, message.Length - 2);
            }

            var projectContent = await GetProjectFileContent(destinationFileName).ConfigureAwait(true);
            return new Result<ProjectFileContent>(message, success, projectContent.Data);
        }

        private Task<Result<Dictionary<string, byte[]>>> GetBsdlFilesAsByteArray(List<BsdlContainer> bsdl)
        {
            Dictionary<string, byte[]> dict = new Dictionary<string, byte[]>();
            bool success = true;
            string message = string.Empty;
            foreach (var entry in bsdl)
            {
                if (!string.IsNullOrEmpty(entry.BSDLFileName))
                {
                    var bsdlResult = manifestHandler.GetObjectFromZipArchive<byte[]>(entry.BSDLFileName);
                    if (bsdlResult != null)
                    {
                        dict.Add(entry.JTAGName, bsdlResult);
                    }
                    else
                    {
                        success = false;
                        message = "Could not read out the BSDL file: " + entry.BSDLFileName;
                    }
                }
            }
            
            if (message.Length > 2)
            {
                message = message.Substring(0, message.Length - 2);
            }
            else
            {
                message = Ok;
            }

            return Task.FromResult(new Result<Dictionary<string, byte[]>>(message, success, dict));
        }

        private Task<Func<List<WrappedBsdlContainer>, Task<Result>>> GetFuncEmpty()
        {
            Func<List<WrappedBsdlContainer>, Task<Result>> func = (bsdl) => Task.FromResult(new Result(Ok, true));
            return Task.FromResult(func);
        }

        private async Task<Result> AddBsdlDataToZipFile(
            List<WrappedBsdlContainer> bsdl)
        {
            if (bsdl == null || !bsdl.Any() || !await AreAnyPathsDefined(bsdl))
            {
                return new Result(Ok, true);
            }

            try
            {
                bool success = true;
                string message = "";
                foreach (var entry in bsdl)
                {
                    if (!string.IsNullOrEmpty(entry.BSDLFileToBeUsed) && File.Exists(entry.BSDLFileToBeUsed))
                    {
                        string bsdlFileLocation = Manifest.BSDLDEF + entry.BSDLFileToBeUsed.Substring(
                            entry.BSDLFileToBeUsed.LastIndexOf("\\") + 1);
                        entry.BsdlContainerToUse.BSDLFileName = bsdlFileLocation;
                        var dataContent = await GetFileContentAsBytes(entry.BSDLFileToBeUsed).ConfigureAwait(true);
                        if (dataContent.Success && dataContent.Data != null)
                        {
                            manifestHandler.SaveBsdlContentIntoProjectFile(entry.BsdlContainerToUse, dataContent.Data);
                        }
                        else if (!dataContent.Success)
                        {
                            success = false;
                            message += dataContent.Message + ", ";
                        }
                    }
                }

                if (!success && message.Length > 2)
                {
                    message = message.Substring(0, message.Length - 2);
                }

                return new Result(message, success);
            }
            catch (Exception e)
            {
                return new Result(e.Message, false);
            }
        }

        private async Task AddFolderByTestTypeIntoZipArchive(ZipFile archive, string folderPath, BoundaryScanTestType testType, string jtagName, SvfPath svfPathNew)
        {
            if (!string.IsNullOrEmpty(folderPath) && Directory.Exists(folderPath))
            {
                string destinationFolder = Manifest.SVFDEF + jtagName + "/" + await GetTestTypeName(testType).ConfigureAwait(true);
                if (!Directory.Exists(Path.Combine(folderPath, "neighbours")))
                {
                    archive.AddDirectory(folderPath, destinationFolder);
                }
                else
                {
                    await CheckIfDirectoryAlreadyExistsInArchive(archive, Manifest.SVFDEF).ConfigureAwait(true);
                    await CheckIfDirectoryAlreadyExistsInArchive(archive, Manifest.SVFDEF + jtagName + "/").ConfigureAwait(true);
                    await CheckIfDirectoryAlreadyExistsInArchive(archive, Manifest.SVFDEF + jtagName + "/"
                        + await GetTestTypeName(testType).ConfigureAwait(true) + "/").ConfigureAwait(true);
                    foreach (var file in Directory.GetFiles(folderPath))
                    {
                        ZipEntry entryToDelete = archive.Entries.FirstOrDefault(x => x.FileName.Equals(Path.Combine(destinationFolder, file).Replace("\\", "/")));
                        if (entryToDelete != null)
                        {
                            archive.RemoveEntry(entryToDelete);
                        }

                        archive.AddFile(file, destinationFolder);
                    }

                    foreach (var file in Directory.GetFiles(Path.Combine(folderPath, "neighbours")))
                    {
                        ZipEntry entryToDelete = archive.Entries.FirstOrDefault(x => x.FileName.Equals(Path.Combine(destinationFolder, file).Replace("\\", "/")));
                        if (entryToDelete != null)
                        {
                            archive.RemoveEntry(entryToDelete);
                        }

                        archive.AddFile(file, destinationFolder);
                    }
                }

                await UpdateNewSvfPath(svfPathNew, destinationFolder, testType);
            }
        }

        private Task UpdateNewSvfPath(SvfPath svfPathNew, string destinationFolder, BoundaryScanTestType testType)
        {
            switch (testType)
            {
                case BoundaryScanTestType.UnknownControl: svfPathNew.UnknownControls = destinationFolder; break;
                case BoundaryScanTestType.UnknownInput: svfPathNew.UnknownInputs = destinationFolder; break;
                case BoundaryScanTestType.Pulldown: svfPathNew.PullDowns = destinationFolder; break;
                case BoundaryScanTestType.Pullup: svfPathNew.PullUps = destinationFolder; break;
                case BoundaryScanTestType.PullupPulldown: svfPathNew.PullUpsAndDowns = destinationFolder; break;
                case BoundaryScanTestType.DirectGnd: svfPathNew.DirectGnd = destinationFolder; break;
                case BoundaryScanTestType.DirectPower: svfPathNew.DirectPower = destinationFolder; break;
                default: break;
            }

            return Task.CompletedTask;
        }

        private Task<bool> AreAnyPathsDefined(List<WrappedBsdlContainer> bsdl)
        {
            foreach (var entry in bsdl)
            {
                if (entry is SvfPathWrappedBsdlContainer svfPathEntry)
                {
                    if (!string.IsNullOrEmpty(entry.BSDLFileToBeUsed)
                        || (svfPathEntry.SvfPathsToUseFromLocalSystem != null
                        && (!string.IsNullOrEmpty(svfPathEntry.SvfPathsToUseFromLocalSystem.DirectPower)
                        || !string.IsNullOrEmpty(svfPathEntry.SvfPathsToUseFromLocalSystem.PullUps)
                        || !string.IsNullOrEmpty(svfPathEntry.SvfPathsToUseFromLocalSystem.DirectGnd)
                        || !string.IsNullOrEmpty(svfPathEntry.SvfPathsToUseFromLocalSystem.PullDowns)
                        || !string.IsNullOrEmpty(svfPathEntry.SvfPathsToUseFromLocalSystem.PullUpsAndDowns)
                        || !string.IsNullOrEmpty(svfPathEntry.SvfPathsToUseFromLocalSystem.UnknownControls)
                        || !string.IsNullOrEmpty(svfPathEntry.SvfPathsToUseFromLocalSystem.UnknownInputs))))
                    {
                        return Task.FromResult(true);
                    }
                }
                else if (entry is SvfDataWrappedBsdlContainer svfFilesEntry)
                {
                    if (svfFilesEntry.SvfDataFiles != null && svfFilesEntry.SvfDataFiles.Count > 0)
                    {
                        return Task.FromResult(true);
                    }
                }

            }

            return Task.FromResult(false);
        }

        private async Task<Result<byte[]>> GetFileContentAsBytes(string filePath)
        {
            try
            {
                return new Result<byte[]>(Ok, true, await File.ReadAllBytesAsync(filePath).ConfigureAwait(true));
            }
            catch (Exception e)
            {
                return new Result<byte[]>(e.Message, false, null);
            }
        }

        private async Task<Result<byte[]>> GetJsonContentAsBytes(object value)
        {
            try
            {
                return await Task.Run<Result<byte[]>>(() =>
                {
                    return new Result<byte[]>(Ok, true, Encoding.ASCII.GetBytes(manifestHandler.GetObjecttAsJsonString(value)));
                }).ConfigureAwait(true);
            }
            catch (Exception e)
            {
                return new Result<byte[]>(e.Message, false, null);
            }
        }

        private Task CheckIfDirectoryAlreadyExistsInArchive(ZipFile archive, string destinationFolder)
        {
            if (!string.IsNullOrEmpty(destinationFolder))
            {
                bool found = false;
                foreach (var entry in archive.Entries)
                {
                    if (entry.FileName.Equals(destinationFolder) || entry.FileName.Replace("\\", "/").Equals(destinationFolder.Replace("\\", "/")))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                {
                    archive.AddDirectoryByName(destinationFolder);
                }
            }

            return Task.CompletedTask;
        }

        private Task<string> GetTestTypeName(BoundaryScanTestType scantTestType)
        {
            switch (scantTestType)
            {
                case BoundaryScanTestType.Pullup: return Task.FromResult("pullUps");
                case BoundaryScanTestType.Pulldown: return Task.FromResult("pullDowns");
                case BoundaryScanTestType.PullupPulldown: return Task.FromResult("pullUpsAndDowns");
                case BoundaryScanTestType.UnknownControl: return Task.FromResult("unknownsControl");
                case BoundaryScanTestType.UnknownInput: return Task.FromResult("unknownsInput");
                case BoundaryScanTestType.DirectGnd: return Task.FromResult("directGnd");
                case BoundaryScanTestType.DirectPower: return Task.FromResult("directPower");
                default: return Task.FromResult("Undefined");
            }
        }

        private Task<Result<byte[]>> GetOdbProject(string odbProject)
        {
            string message = Ok;
            bool success = false;
            byte[] data = null;
            try
            {
                using ZipFile file = manifestHandler.GetArchiveForPath(manifestHandler.ManifestProjectFilePath);
                if (file == null)
                {
                    return Task.FromResult(new Result<byte[]>("Error getting zip archive for: " + manifestHandler.ManifestProjectFilePath, false, null));
                }

                bool odbFound = false;
                foreach (var entry in file.Entries)
                {
                    if (entry.FileName.Contains(Manifest.ODBPROJECTDEF))
                    {
                        odbFound = true;
                        break;
                    }
                }

                if (!odbFound)
                {
                    return Task.FromResult(new Result<byte[]>(Ok, true, null));
                }

                List<ZipEntry> entriesToDelete = new List<ZipEntry>();
                foreach (var entry in file.Entries)
                {
                    if ((entry.FileName.Equals(odbProject) || entry.FileName.Replace("/", "\\").Equals(odbProject))
                        || (!entry.FileName.Contains(odbProject) && !entry.FileName.Replace("/", "\\").Contains(odbProject)))
                    {
                        entriesToDelete.Add(entry);
                    }
                }

                file.RemoveEntries(entriesToDelete);
                List<ZipEntry> entriesToModify = new List<ZipEntry>(file.Entries);
                foreach (var entry in entriesToModify)
                {
                    entry.FileName = entry.FileName.Replace(odbProject, "");
                }

                MemoryStream memStream = new MemoryStream();
                file.Save(memStream);
                data = memStream.ToArray();
                success = true;
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return Task.FromResult(new Result<byte[]>(message, success, data));
        }
    }
}
