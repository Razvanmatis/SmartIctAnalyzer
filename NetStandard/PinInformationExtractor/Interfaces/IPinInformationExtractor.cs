using Interfaces.PcbInvestigator;
using PinInformationExtractor.Helper;
using System.Collections.Generic;

namespace PinInformationExtractor.Interfaces
{ 
    public interface IPinInformationExtractor
    {
        List<JtagConnectionInfo> CreatePinInformationFile(string filePath, IList<IPCBComponent> ics, IList<IPCBComponent> pullUps, IList<IPCBComponent> pullDowns,
            string jtagPinIdentifier = PinInformationExtractor.Implementations.PinInformationExtractor.DEFJTAGPINIDENTIFIER);
        List<string> GetAllGroundPins(List<PinConnectionInfo> pins);
        List<string> GetAllPowerPins(List<PinConnectionInfo> pins);
        List<JtagConnectionInfo> GetAllPinInformation(IList<IPCBComponent> ics, IList<IPCBComponent> pullUps, IList<IPCBComponent> pullDowns,
            string jtagPinIdentifier = PinInformationExtractor.Implementations.PinInformationExtractor.DEFJTAGPINIDENTIFIER);
        List<string> GetAllUnknownPins(List<PinConnectionInfo> pins);
    }
}
