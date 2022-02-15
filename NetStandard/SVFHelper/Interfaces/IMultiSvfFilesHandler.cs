using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Interfaces
{
    public interface IMultiSvfFilesHandler
    {
        List<ISvfData> GetNewSvfFilesOfMultiSvfs(List<ISvfData> svfData);
    }
}
