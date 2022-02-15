using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.TestCoverage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProMik.SmartIct.Console.JtagPinInformationExtractor.Interfaces
{
    public interface IJtagPinInformationExtractor
    {
        Task<List<JtagConnectionInfo>> GetAllPinInformation(
            ITestCoverageDataModel testCoverageData,
            TestCoverageSettings settings,
            List<IPCBComponent> allComponents,
            Action<string, LogCategory> loggerAction);
    }
}
