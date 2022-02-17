using ProMik.Core.Interfaces.Results;
using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Console.ProjectFileHandler.Helper;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.Svf.Contracts;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ProMik.SmartIct.Console.ProjectFileHandler.Interfaces
{
    public interface IProjectFileHandler
    {
        Task<Result<byte[]>> GetOdbProjectZipFolder(string svfProjectFilePath);

        Task<Result<byte[]>> GetJsonProjectFile(string svfProjectFilePath);

        Task<Result<byte[]>> GetSettingsFile(string svfProjectFilePath);

        Task<Result<SettingsContent>> GetSettingsContent(string svfProjectFilePath);

        Task<Result<byte[]>> GetBomFile(string svfProjectFilePath);

        Task<Result<BomSettings>> GetBomSettingsContent(string svfProjectFilePath);

        Task<Result<byte[]>> GetBomSettingsFile(string svfProjectFilePath);

        Task<Result<Dictionary<string, byte[]>>> GetBsdlFilesContent(string svfProjectFilePath);

        Task<Result<Dictionary<string, Stream>>> GetBsdlFiles(string svfProjectFilePath);

        Task<Result<Manifest>> GetManifestContent(string svfProjectFilePath);

        Task<Result<ProjectFileContent>> CreateProjectFile(
            string odbProjectFolderPath,
            byte[] jsonProject,
            SettingsContent settingsJsonContent,
            string bomFilePath,
            BomSettings bomSettings,
            List<SvfPathWrappedBsdlContainer> bsdl,
            string destinationFileName);

        Task<Result<ProjectFileContent>> CreateProjectFile(
            string odbProjectFolderPath,
            byte[] jsonProject,
            SettingsContent settingsJsonContent,
            string bomFilePath,
            BomSettings bomSettings,
            List<SvfDataWrappedBsdlContainer> bsdl,
            string destinationFileName);

        Task<Result<ProjectFileContent>> CreateProjectFile(
            string odbProjectFolderPath,
            byte[] jsonProject,
            SettingsContent settingsJsonContent,
            string bomFilePath,
            BomSettings bomSettings,
            List<WrappedBsdlContainer> bsdl,
            string destinationFileName);

        Task<Result<ProjectFileContent>> GetProjectFileContent(string svfProjectFilePath);

        Task<Result> UpdateOdbProjectZipFolder(string svfProjectFilePath, string odbPath);

        Task<Result> UpdateJsonProjectFile(string svfProjectFilePath, byte[] jsonData);

        Task<Result> UpdateSettingsContent(string svfProjectFilePath, SettingsContent settingsContent);

        Task<Result> UpdateBomFile(string svfProjectFilePath, byte[] bomContent);

        Task<Result> UpdateBomSettingsContent(string svfProjectFilePath, BomSettings bomSettings);

        Task<Result> UpdateBsdlContent(string svfProjectFilePath, List<WrappedBsdlContainer> bsdlContent);

        Task<Result> UpdateManifestContent(string svfProjectFilePath, Manifest manifest);

        Task<Result> UpdateSvfFiles(string svfProjectFilePath, List<ISvfData> svfFiles, bool firstDeleteAllContent = true);

        Task<Result> UpdateSvfFiles(string svfProjectFilePath, string jtagName, SvfPath svfPathsToUse, bool firstDeleteAllContent = true);
    }
}
