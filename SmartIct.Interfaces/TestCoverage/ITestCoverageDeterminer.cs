using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Interfaces.PcbInvestigator;
using Interfaces.TestCoverage;

namespace Interfaces.TestCoverage
{
    public interface ITestCoverageDeterminer
    {
        bool IsTestResultAvailableForComponentsAndType(IPCBComponent comp, IPCBComponent second);

        IList<IPCBComponent> DefinePullDownResistors(IList<INetComponent> gndNets = null);

        IList<IPCBComponent> DefineIcs(IList<INetComponent> jtagNets = null);

        IList<IPCBComponent> DefinePullUpResistors(IList<INetComponent> powerNets = null);

        IList<INetComponent> DefineGndNets(IList<INetComponent> nets, string gndIdentifier, string gndBlacklist);

        IList<INetComponent> DefinePowerNets(IList<INetComponent> nets, string powerIdentifier, string powerBlacklist);

        IList<INetComponent> DefineJtagNets(IList<INetComponent> nets, string jtagIdentifier, string jtagBlacklist);

        void ClearAllObjects();

        IList<ITestCoverageResult> DefineTestCoverageForIcObjects(IList<IPCBComponent> ics = null);

        Task<IList<ITestCoverageResult>> DefineTestCoverageForPullUpDownObjects(IList<IPCBComponent> ics = null, IList<IPCBComponent> pullUpsDowns = null, bool usePullDown = true);

        Task<float> GetTestCoveragePercentageValueForAllOtherObjects(IList<IPCBComponent> ics = null);

        Task<float> GetTestCoveragePercentageValue(IList<IPCBComponent> pullUpsDowns = null, IList<IPCBComponent> ics = null);

        Task<float> GetTestCoveragePercentageValueForIcs(IList<IPCBComponent> ics = null);

        void SetTestsPerformedState(TestCoverageObject objectType, bool value);

        bool GetTestsPerformedState(TestCoverageObject objectType);

        IList<IPCBComponent> DefineOthers();
    }
}
