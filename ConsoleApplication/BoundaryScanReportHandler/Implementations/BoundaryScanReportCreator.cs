using ProMik.Core.Interfaces.Results;
using ProMik.SmartIct.Console.BoundaryScanReportCreator.Interfaces;
using ProMik.SmartIct.Console.Contracts.Implementations;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Services.ReportCreator.Implementations;
using ProMik.SmartIct.Services.ReportCreator.Interfaces;
using ProMik.SmartIct.Svf.SvfFileCreation.Implementations;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProMik.SmartIct.Console.BoundaryScanReportCreator.Implementations
{
    public class BoundaryScanReportCreator : IBoundaryScanReportCreator
    {
        private const string Ok = "Ok";
        private readonly ILogger logger = new ConsoleLogger(false);
        private readonly ISvfDataCreator svfDataCreator;
        private readonly ICsvReportCreator csvReportHandler;

        public BoundaryScanReportCreator()
        {
            svfDataCreator = new SvfDataCreator(logger);
            csvReportHandler = new CsvReportCreator(logger, null);
        }

        public async Task<Result> CreateCsvReport(
            JtagConnectionInfo jtag,
            List<ISvfData> svfFiles,
            List<PinTestTypeContainer> pinInformation,
            string jtagName,
            string destinationFileName,
            Action<string, LogCategory> loggerAction)
        {
            bool success = true;
            string message = Ok;
            ((ConsoleLogger)logger).SetLoggerCallbackForError((msg, cat) =>
            {
                loggerAction?.Invoke(msg, cat);
                if (cat == LogCategory.ERROR)
                {
                    success = false;
                    message = msg;
                }
            });

            await Task.Run(() =>
            {
                List<ISvfData> svfFilesToUse = new List<ISvfData>(svfFiles);
                List<PinTestTypeContainer> pinInformationToUse = new List<PinTestTypeContainer>(pinInformation);
                svfDataCreator.UpdateTestTypeInternal(pinInformationToUse, svfFilesToUse);
                pinInformationToUse.RemoveAll(svf => !svfFilesToUse.Select(x => x.PinName).Contains(svf.PinTestObject.PinNumber));
                csvReportHandler.UpdateReportContent(
                    jtagName,
                    pinInformationToUse,
                    jtagName,
                    svfDataCreator.GetAllUntestedPins(jtag.Pins, pinInformationToUse));
                csvReportHandler.CreateReportFile(destinationFileName);
            }).ConfigureAwait(true);
           
            return new Result(message, success);
        }
    }
}
