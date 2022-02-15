using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.TestCoverage;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProMik.SmartIct.Console.BoundaryScanObjectDetermination.Interfaces
{
    public interface IBoundaryScanObjectDetermination
    {
        Task<ITestCoverageDataModel> GetBoundaryScanRelatedObjects(
            IList<INetComponent> allNets,
            TestCoverageSettings settings,
            bool removeAllComponentsWithoutAnyValues);
    }
}
