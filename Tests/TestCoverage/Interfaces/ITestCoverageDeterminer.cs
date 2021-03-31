using System.Collections.Generic;
using System.Threading.Tasks;
using Interfaces.PcbInvestigator;
using TestCoverage.Helper;

namespace TestCoverage.Interfaces
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

        Task<TestCoverageItems> GetCompleteTestCoverageOfAllJtags(TestCoverageItems items);

        Task<TestCoverageItems> TestCoverageForAllOthers(TestCoverageItems items);

        Task<TestCoverageItems> TestCoverageForPullUpsDowns(TestCoverageItems items);

        Task<TestCoverageItems> TestCoverageForPullDowns(TestCoverageItems items);

        Task<TestCoverageItems> TestCoverageForPullUps(TestCoverageItems items);

        Task<TestCoverageItems> TestCoverageForIcs(TestCoverageItems items);

        TestCoverageItems RunTheTestCoverageForDeterminingObjects(
            IList<INetComponent> nets,
            IdentifierBlacklistContainer content,
            TestCoverageItems items,
            ref IList<IPCBComponent> ics,
            ref IList<IPCBComponent> pullUps,
            ref IList<IPCBComponent> pullDowns);

        IList<IPCBComponent> DefineOthers();
    }
}
