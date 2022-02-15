using ProMik.SmartIct.TestCoverageDeterminer.Helper;
using System;

namespace ProMik.SmartIct.TestCoverageDeterminer.Interfaces
{
    public interface ITestCoverageHandler
    {
        void DetermineObjects(Action<TestCoverageItems> setItemsObjectsAction, Func<TestCoverageItems> getActualItemsObjectFunc);
    }
}
