using ProMik.SmartIct.Interfaces.PcbInvestigator;

namespace ProMik.SmartIct.TestCoverageDeterminer.Interfaces
{
    public enum TestCoverageObject
    {
        /// <summary>
        /// PULLUP
        /// </summary>
        PULLUP,

        /// <summary>
        /// PULLDOWN
        /// </summary>
        PULLDOWN,

        /// <summary>
        /// JTAG
        /// </summary>
        JTAG,

        /// <summary>
        /// The other components
        /// </summary>
        OTHERS,
    }

    public interface ITestCoverageResult
    {
        IPCBComponent PCBComponent { get; }

        float TestCoverage { get; }
    }
}
