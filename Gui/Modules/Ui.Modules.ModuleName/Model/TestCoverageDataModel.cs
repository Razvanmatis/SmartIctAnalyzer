using System.Collections.Generic;
using Interfaces.PcbInvestigator;
using TestCoverage.Interfaces;

namespace Ui.Modules.ModuleName.Model
{
    public class TestCoverageDataModel : ITestCoverageDataModel
    {
        public IList<IPCBComponent> Ics { get; set; }

        public IList<IPCBComponent> PullUps { get; set; }

        public IList<IPCBComponent> PullDowns { get; set; }
    }
}
