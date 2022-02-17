using System.Collections.Generic;
using System.Threading.Tasks;
using ProMik.SmartIct.Services.ManifestHandler.Interfaces;
using ProMik.Svf.Contracts;
using Ui.Modules.ModuleName.Helper;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IManifestHandler : IProjectHandler, IManifestHandling
    {
        Dictionary<BsdlContainer, byte[]> BsdlContent { get; }

        Manifest GetActuallyLoadedManifest();

        bool UpdateJsonProject(byte[] fileContent, string newFileName = "");

        bool UpdateSettingsFile(byte[] settings, string newFileName = "", bool useOnlyJsonSettingsFileAsByteContent = false);

        bool IsManifestHandlingActive();

        string GetProjectSelectionPath();

        bool SaveAllBsdlContentsIntoProjectFile(string selection);

        bool AddBsdlContentToDict(string bsdlFile, string jtagDevice, out BsdlContainer bsdl, out byte[] data);

        void UpdatePgmSettings(string jtagDevice, ProgrammerSettings pgmSettings);

        void UpdateBsdlParameter(
            string jtagName,
            uint boundaryScanLength,
            uint instructionLength,
            uint idCodeRegister,
            uint idCodeInstruction,
            uint preloadInstruction);
    }
}
