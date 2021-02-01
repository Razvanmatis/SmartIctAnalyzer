using System;
using Interfaces.PcbInvestigator;
using Interfaces.TestCoverage;

namespace TestCoverage
{
    public class TestCoverageResult : ITestCoverageResult
    {
        private IPCBComponent pcbComponent;
        private float testCoverage;

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
