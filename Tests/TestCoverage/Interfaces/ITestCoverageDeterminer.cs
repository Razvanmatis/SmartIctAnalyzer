using System.Collections.Generic;
using System.Threading.Tasks;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.TestCoverage;
using ProMik.SmartIct.TestCoverageDeterminer.Helper;

namespace ProMik.SmartIct.TestCoverageDeterminer.Interfaces
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

        /// <summary>
        /// none connections
        /// </summary>
        BOUNDARY_SCAN_NONE,
    }

    public interface ITestCoverageDeterminer
    {
        bool IsTestResultAvailableForComponentsAndType(IPCBComponent comp, IPCBComponent second);

        void ClearAllObjects();

        Task<TestCoverageItems> GetTestCoverage(TestCoverageItems items, TestCoverageType type);

        TestCoverageItems DetermineTestCoverageRelatedObjects(
            IList<INetComponent> nets,
            IdentifierBlacklistContainer content,
            TestCoverageItems items);

        ITestCoverageDataModel GetTestRelatedObjects(
            IList<INetComponent> nets,
            IdentifierBlacklistContainer content);

        Task<float> GetTestCoverageValue(TestCoverageType type, List<IPCBComponent> jtags = null);

        int GetAmountOfTotalDistinctNets(
            TestCoverageType type, bool onlyIntersectionWithIcNets = true, List<IPCBComponent> jtags = null);
    }
}
