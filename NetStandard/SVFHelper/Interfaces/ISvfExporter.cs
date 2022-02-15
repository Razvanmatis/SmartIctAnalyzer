using ProMik.SmartIct.Services.ManifestHandler.Interfaces;
using ProMik.SmartIct.Svf.SvfFileCreation.Helper;
using ProMik.SmartIct.Svf.SvfInterfaces.Container;
using System.Collections.Generic;
using System.IO;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Interfaces
{
    public interface ISvfExporter : IsvfDataCollector
    {
        string GetBasePath();

        WrappedProgrammerSettings GetPgmSettings(
            string jtagDevice, WrappedProgrammerSettings defSettings, bool askForSvfSettings);

        bool CheckJtagDestinationLocation(string basePath, string jtagName, string selectedJtag, List<string> allJtags);

        string SelectJtagsToBeExported(List<string> jtags);

        bool IsJtagIncludedForExport(string jtag, string selectedJtag);
    }
}
