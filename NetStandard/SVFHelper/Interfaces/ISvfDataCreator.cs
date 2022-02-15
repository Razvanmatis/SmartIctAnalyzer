using ProMik.Core.Interfaces.Bsdl;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Svf.SvfFileCreation.Helper;
using ProMik.SmartIct.Svf.SvfInterfaces.Container;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Interfaces
{
    public interface ISvfDataCreator
    {
        SvfCreationResult GetAllSvfDataFiles(
            JtagConnectionInfo jtag,
            IBSDLPackage pinPackage,
            IBSDLOutput bsdlContent,
            byte[] defaultVector,
            string basePath = "");

        bool CheckPinConsistency(JtagConnectionInfo jtag, IBSDLOutput package);

        void PerformChecks(
           uint idCode,
           IBSDLOutput bsdlResult,
           uint jtagIdCode,
           int amountToSkip,
           uint scanChainLength,
           byte[] defaultVector);

        void UpdateTestTypeInternal(List<PinTestTypeContainer> allPins, List<ISvfData> svfResult);

        List<PinTestTypeContainer> GetAllUntestedPins(List<PinConnectionInfo> pins, List<PinTestTypeContainer> allTestedPins);
    }
}
