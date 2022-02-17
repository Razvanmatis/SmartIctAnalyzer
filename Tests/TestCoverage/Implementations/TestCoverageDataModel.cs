using System.Collections.Generic;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.TestCoverage;

namespace ProMik.SmartIct.TestCoverageDeterminer.Implementations
{
    public class TestCoverageDataModel : ITestCoverageDataModel
    {
        public IList<IPCBComponent> Jtags { get; set; }

        public IList<IPCBComponent> PullUps { get; set; }

        public IList<IPCBComponent> PullDowns { get; set; }

        public IList<IPCBComponent> Others { get; set; }
    }
}
