using System.Collections.Generic;

namespace SVFHelper.Interfaces
{
    public interface ISvfWriter
    {
        List<ISvfData> GetSVFFiles(string basePath, string jtagName, List<string> pinLabel, ProMik.BSDL.Interfaces.Entities.IBSDLOutput bsdlOutput, byte[] defaulVector);
    }
}
