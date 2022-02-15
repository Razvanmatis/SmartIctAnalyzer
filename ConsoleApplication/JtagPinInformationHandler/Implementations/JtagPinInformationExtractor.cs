using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Console.Contracts.Implementations;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.TestCoverage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProMik.SmartIct.Console.JtagPinInformationExtractor.Implementations
{
    public class JtagPinInformationExtractor : Interfaces.IJtagPinInformationExtractor
    {
        private ILogger logger = new ConsoleLogger(false);
        private ProMik.SmartIct.JtagPinInformationExtractor.Interfaces.IJtagPinInformationCreator pinInformationExtractor;

        public JtagPinInformationExtractor()
        {
            pinInformationExtractor = new ProMik.SmartIct.JtagPinInformationExtractor.Implementations.JJtagPinInformationCreator(logger);
        }

        public async Task<List<JtagConnectionInfo>> GetAllPinInformation(
            ITestCoverageDataModel testCoverageData,
            TestCoverageSettings settings,
            List<IPCBComponent> allComponents,
            Action<string, LogCategory> loggerAction)
        {
            ((ConsoleLogger)logger).SetLoggerCallbackForError(loggerAction);
            return await Task.Run<List<JtagConnectionInfo>>(() =>
           {
               return pinInformationExtractor.GetAllPinInformation(
                   testCoverageData.Jtags,
                   testCoverageData.PullUps,
                   testCoverageData.PullDowns,
                   testCoverageData.Others,
                   settings.GndIdentifier.Split(";").ToList(),
                   settings.PowerIdentifier.Split(";").ToList(),
                   allComponents,
                   settings.JtagIdentifier.Split(";").ToList());
           }).ConfigureAwait(true);
        }
    }
}
