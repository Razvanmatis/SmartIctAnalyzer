using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.TestCoverageDeterminer.Interfaces;

namespace ProMik.SmartIct.TestCoverageDeterminer.Implementations
{
    public class TestCoverageResult : ITestCoverageResult
    {
        private readonly IPCBComponent pcbComponent;
        private readonly float testCoverage;

        public TestCoverageResult(IPCBComponent pcbComponent, float testCoverage)
        {
            this.pcbComponent = pcbComponent;
            this.testCoverage = testCoverage;
        }

        public IPCBComponent PCBComponent
        {
            get
            {
                return pcbComponent;
            }
        }

        public float TestCoverage
        {
            get
            {
                return testCoverage;
            }
        }
    }
}
