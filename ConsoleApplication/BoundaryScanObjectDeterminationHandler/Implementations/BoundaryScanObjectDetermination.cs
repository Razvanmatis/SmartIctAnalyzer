using BoundaryScanObjectDeterminationHandler.Implementations;
using ProMik.SmartIct.Console.BoundaryScanObjectDetermination.Interfaces;
using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.TestCoverage;
using ProMik.SmartIct.TestCoverageDeterminer.Helper;
using ProMik.SmartIct.TestCoverageDeterminer.Implementations;
using ProMik.SmartIct.TestCoverageDeterminer.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProMik.SmartIct.Console.BoundaryScanObjectDetermination.Implementations
{
    public class BoundaryScanObjectDetermination : IBoundaryScanObjectDetermination
    {
        private ITestCoverageDeterminer testCoverageDeterminer;

        public async Task<ITestCoverageDataModel> GetBoundaryScanRelatedObjects(
            IList<INetComponent> allNets,
            TestCoverageSettings settings,
            bool removeAllComponentsWithoutAnyValues)
        {
            testCoverageDeterminer = new TestCoverageBoundaryScanDeterminer(new PresetBomUseValues() { UseValues = removeAllComponentsWithoutAnyValues});
            return await Task.Run<ITestCoverageDataModel>(() =>
            {
                return testCoverageDeterminer.GetTestRelatedObjects(
                    allNets,
                    new IdentifierBlacklistContainer(
                        settings.GndIdentifier,
                        settings.GndBlacklist,
                        settings.PowerIdentifier,
                        settings.PowerBlacklist,
                        settings.JtagIdentifier,
                        settings.JtagBlacklist));
            });
        }
    }
}
