using System;
using Interfaces.Gui;
using TestCoverage.Helper;
using TestCoverage.Interfaces;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class TestCoverageHandler : ITestCoverageHandler
    {
        private readonly ILogger logger;
        private readonly ISettingsStorageManager settingsStorageManager;
        private readonly IResultModel resultModel;
        private readonly ITestCoverageDeterminer testCoverageDeterminer;

        public TestCoverageHandler(
            ILogger logger,
            ISettingsStorageManager settingsStorageManager,
            IResultModel resultModel,
            ITestCoverageDeterminer testCoverageDeterminer)
        {
            this.logger = logger;
            this.testCoverageDeterminer = testCoverageDeterminer;
            this.resultModel = resultModel;
            this.settingsStorageManager = settingsStorageManager;
        }

        public void DetermineObjects(Action<TestCoverageItems> setItemsObjectsAction, Func<TestCoverageItems> getActualItemsObjectFunc)
        {
            if (resultModel.Result != null && resultModel.Result.Nets != null && resultModel.Result.Nets.Count > 0)
            {
                var content = settingsStorageManager.GetStorageContent();
                setItemsObjectsAction(testCoverageDeterminer.DetermineBoundaryScanObjects(
                    resultModel.Result.Nets,
                    new IdentifierBlacklistContainer(
                        content.GndNetIdentifier,
                        content.GndNetBlacklist,
                        content.PowerNetIdentifier,
                        content.PowerNetBlacklist,
                        content.JTAGNetIdentifier,
                        content.JTAGNetBlacklist),
                    getActualItemsObjectFunc()));
            }
            else
            {
                logger.LogMessage("First load data into tool!", LogCategory.WARNING);
            }
        }
    }
}
