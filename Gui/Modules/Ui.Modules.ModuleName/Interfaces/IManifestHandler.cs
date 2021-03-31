using System.Threading.Tasks;
using Interfaces.Helper;
using Ui.Modules.ModuleName.Helper;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IManifestHandler
    {
        void SetPathAndManifestOfZipfile(string path, bool newCreation);

        bool UpdateManifestInZipfile(Manifest manifest);

        Manifest GetActuallyLoadedManifest();

        byte[] GetSettingsFile();

        Task<string> GetOdbProjectPath();

        byte[] GetJsonProjectFile();

        string GetBomFile();

        BomSettings GetBomSettingsFile();

        Task<bool> UpdateOdbProject(string pathToFolder);

        bool UpdateJsonProject(byte[] fileContent, string newFileName = "");

        bool UpdateSettingsFile(byte[] settings, string newFileName = "");

        bool UpdateBomFile(byte[] fileContent, string newFileName);

        bool UpdateBomSettingsFile(BomSettings fileContent, string newFileName);

        bool IsManifestHandlingActive();

        void ResetValues();

        string GetProjectSelectionPath();
    }
}
