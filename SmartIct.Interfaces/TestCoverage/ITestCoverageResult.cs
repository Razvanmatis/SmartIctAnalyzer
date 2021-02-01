using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.PcbInvestigator;

namespace Interfaces.TestCoverage
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
    }

    public interface ITestCoverageResult
    {
        IPCBComponent PCBComponent { get; }

        float TestCoverage { get; }
    }
}
