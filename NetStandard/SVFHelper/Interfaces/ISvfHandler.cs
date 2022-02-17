using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Interfaces
{
    public interface ISvfHandler
    {
        void HandleSvfFileGeneration(ISvfExporter projectHandlerToUse, string jtagPinInformation = "");

        void PlaySvfFileHandler(ISvfExporter projectHandlerToUse, bool justFiles);

        void SaveCreatedSvfFilesIntoProject(string jtagSelection);

        Dictionary<string, List<ISvfData>> SavedSvfData { get; }
    }
}
