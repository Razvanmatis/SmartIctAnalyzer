using System.Collections.Generic;

namespace SVFHelper.Interfaces
{
    public interface ISVFHelper
    {
        uint GetBoundaryScanChainLength(List<ProMik.BSDL.Interfaces.Entities.IBSDLAttribute> attributes);

        ISvfData GetSvfContent(string basePath, string jtagName, string pinLabel, ProMik.BSDL.Interfaces.Entities.IBSDLOutput bsdlOutput, bool requestValue, bool expectedValue, byte[] defaultVector);

        ISvfData GetSvfContent(string basePath, string jtagName, List<string> pinLabels, ProMik.BSDL.Interfaces.Entities.IBSDLOutput bsdlOutput, bool requestValue, bool expectedValue, byte[] defaultVector, List<string> notFoundPins = null, List<string> foundPins = null);
    }
}
