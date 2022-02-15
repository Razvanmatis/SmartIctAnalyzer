using System.Collections.Generic;
using ProMik.SmartIct.Interfaces.PcbInvestigator;

namespace ProMik.SmartIct.Interfaces.TestCoverage
{
    public interface ITestCoverageDataModel
    {
        IList<IPCBComponent> Jtags { get; set; }

        IList<IPCBComponent> PullUps { get; set; }

        IList<IPCBComponent> PullDowns { get; set; }

        IList<IPCBComponent> Others { get; set; }
    }
}
