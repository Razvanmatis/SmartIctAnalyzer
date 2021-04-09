using System;
using System.Collections.Generic;
using System.Text;
using TestCoverage.Helper;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface ITestCoverageHandler
    {
        void DetermineObjects(Action<TestCoverageItems> setItemsObjectsAction, Func<TestCoverageItems> getActualItemsObjectFunc);
    }
}
