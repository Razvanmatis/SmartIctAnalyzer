using System.Collections.Generic;
using Interfaces.PcbInvestigator;

namespace TestCoverage.Interfaces
{
    public interface ITestCoverageDataModel
    {
        IList<IPCBComponent> Ics { get; set; }

        IList<IPCBComponent> PullUps { get; set; }

        IList<IPCBComponent> PullDowns { get; set; }
    }
}
