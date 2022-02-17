using ProMik.SmartIct.Interfaces.Helper;
using System.Collections.Generic;

namespace ProMik.SmartIct.Services.ReportCreator.Interfaces
{
    public interface IReportHandler
    {
        void CreateReportFile(string destinationFileName = "");

        void UpdateReportContent(string jtagDevice, List<PinTestTypeContainer> pinContainer, string realJtagDevice, List<PinTestTypeContainer> pinContainerNotTested);

        Dictionary<string, List<PinTestTypeContainer>> ReportContent { get; }
    }
}
