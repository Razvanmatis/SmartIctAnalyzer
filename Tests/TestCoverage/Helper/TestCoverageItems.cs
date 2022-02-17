namespace ProMik.SmartIct.TestCoverageDeterminer.Helper
{
    public class TestCoverageItems
    {
        public TestCoverageItems(
            string itemTestCoverageObjects,
            string itemTestCoverageAll,
            string itemTestCoverageJtag,
            string itemTestCoverageOthers,
            string itemTestCoveragePulldown,
            string itemTestCoveragePullup,
            string itemTestCoveragePullupdown,
            bool testCoverageVisibilty,
            float testCoverage,
            bool testCoverageMenuItemsEnabled)
        {
            ItemTestCoverageObjects = itemTestCoverageObjects;
            ItemTestCoverageAll = itemTestCoverageAll;
            ItemTestCoverageJtag = itemTestCoverageJtag;
            ItemTestCoverageOthers = itemTestCoverageOthers;
            ItemTestCoveragePulldown = itemTestCoveragePulldown;
            ItemTestCoveragePullup = itemTestCoveragePullup;
            ItemTestCoveragePullupdown = itemTestCoveragePullupdown;
            TestCoverageVisibilty = testCoverageVisibilty;
            TestCoverage = testCoverage;
            TestCoverageMenuItemsEnabled = testCoverageMenuItemsEnabled;
        }

        public string ItemTestCoverageObjects { get; set; }

        public string ItemTestCoverageAll { get; set; }

        public string ItemTestCoverageJtag { get; set; }

        public string ItemTestCoverageOthers { get; set; }

        public string ItemTestCoveragePulldown { get; set; }

        public string ItemTestCoveragePullup { get; set; }

        public string ItemTestCoveragePullupdown { get; set; }

        public bool TestCoverageVisibilty { get; set; }

        public float TestCoverage { get; set; }

        public bool TestCoverageMenuItemsEnabled { get; set; }
    }
}
