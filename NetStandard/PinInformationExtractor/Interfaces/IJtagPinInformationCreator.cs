using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.JtagPinInformationExtractor.Helper;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using System.Collections.Generic;

namespace ProMik.SmartIct.JtagPinInformationExtractor.Interfaces
{
    public interface IJtagPinInformationCreator
    {
        List<JtagConnectionInfo> CreatePinInformationFile(
            string filePath,
            IList<IPCBComponent> jtags,
            IList<IPCBComponent> pullUps,
            IList<IPCBComponent> pullDowns,
            IList<IPCBComponent> others,
            List<string> gndNets,
            List<string> powerNets,
            List<IPCBComponent> allComponents,
            List<string> jtagPinIdentifier);

        List<JtagConnectionInfo> GetAllPinInformation(
            IList<IPCBComponent> jtags,
            IList<IPCBComponent> pullUps,
            IList<IPCBComponent> pullDowns,
            IList<IPCBComponent> others,
            List<string> gndNets,
            List<string> powerNets,
            List<IPCBComponent> allComponents,
            List<string> jtagPinIdentifier);
    }
}
