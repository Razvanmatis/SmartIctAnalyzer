using System.Collections.Generic;
using System.Threading.Tasks;
using Interfaces.PcbInvestigator;
using TestCoverage.Helper;

namespace TestCoverage.Interfaces
{
    public enum TestCoverageType
    {
        /// <summary>
        /// BOUNDARY_SCAN_ALL
        /// </summary>
        BOUNDARY_SCAN_ALL,

        /// <summary>
        /// BOUNDARY_SCAN_OTHERS
        /// </summary>
        BOUNDARY_SCAN_OTHERS,

        /// <summary>
        /// BOUNDARY_SCAN_PULL_UPS
        /// </summary>
        BOUNDARY_SCAN_PULL_UPS,

        /// <summary>
        /// BOUNDARY_SCAN_PULL_DOWNS
        /// </summary>
        BOUNDARY_SCAN_PULL_DOWNS,

        /// <summary>
        /// BOUNDARY_SCAN_PULL_UPS_DOWNS
        /// </summary>
        BOUNDARY_SCAN_PULL_UPS_DOWNS,

        /// <summary>
        /// BOUNDARY_SCAN_ICS
        /// </summary>
        BOUNDARY_SCAN_ICS,
    }

    public interface ITestCoverageDeterminer
    {
        bool IsTestResultAvailableForComponentsAndType(IPCBComponent comp, IPCBComponent second);

        void ClearAllObjects();

        Task<TestCoverageItems> GetTestCoverage(TestCoverageItems items, TestCoverageType type);

        TestCoverageItems DetermineBoundaryScanObjects(
            IList<INetComponent> nets,
            IdentifierBlacklistContainer content,
            TestCoverageItems items);
    }
}
