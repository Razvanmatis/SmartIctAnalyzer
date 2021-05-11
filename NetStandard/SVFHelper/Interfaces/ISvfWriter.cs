using ProMik.BSDL.Interfaces.Entities;
using System.Collections.Generic;

namespace SVFHelper.Interfaces
{
    public interface ISvfWriter
    {
        List<ISvfData> GetSVFFiles(string basePath, string jtagName, List<string> pinLabel, IBSDLOutput bsdlOutput, byte[] defaultVector);
    }
}
