using ProMik.Core.Interfaces.Results;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProMik.SmartIct.Console.BoundaryScanReportCreator.Interfaces
{
    public interface IBoundaryScanReportCreator
    {
        Task<Result> CreateCsvReport(
            JtagConnectionInfo jtag,
            List<ISvfData> svfFiles,
            List<PinTestTypeContainer> pinInformation,
            string jtagName,
            string destinationFileName,
            Action<string, LogCategory> loggerAction);
    }
}
