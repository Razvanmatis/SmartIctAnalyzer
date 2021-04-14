using System;
using TestCoverage.Helper;

namespace TestCoverage.Interfaces
{
    public interface ITestCoverageHandler
    {
        void DetermineObjects(Action<TestCoverageItems> setItemsObjectsAction, Func<TestCoverageItems> getActualItemsObjectFunc);
    }
}
