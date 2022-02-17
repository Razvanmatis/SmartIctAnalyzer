using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProMik.Core.Interfaces.Events;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.TestCoverage;
using ProMik.SmartIct.TestCoverageDeterminer.Events;
using ProMik.SmartIct.TestCoverageDeterminer.Helper;
using ProMik.SmartIct.TestCoverageDeterminer.Interfaces;

namespace ProMik.SmartIct.TestCoverageDeterminer.Implementations
{
    public class TestCoverageBoundaryScanDeterminer : ITestCoverageDeterminer
    {
        public const string ITEMNOTSELECTED = "CheckboxBlankOutline";
        public const string ErrorMessageDoublesFound
            = "Detected the following resistors as PULLUPS and PULLDOWNS! Please check your settings: ";

        public const string ITEMSELECTED = "Checkbox";
        private static readonly string[] TESTCOVERAGEOBJECTS = new string[] { "Pullups", "Pulldowns", "JTAG", "Others" };
        private readonly IList<ITestCoverageResult> testCoverageResult = new List<ITestCoverageResult>();
        private readonly IEventService eventService;
        private readonly IBomUseValues bomUseValues;
        private readonly ILogger logger;
        private readonly ITestCoverageDataModel testCoverageDataModel;
        private readonly IDialogSelector dialogSelector;
        private IList<IPCBComponent> pullDowns = new List<IPCBComponent>();
        private IList<IPCBComponent> pullUps = new List<IPCBComponent>();
        private IList<IPCBComponent> others = new List<IPCBComponent>();
        private IList<IPCBComponent> jtags = new List<IPCBComponent>();
        private IList<INetComponent> gndNets = new List<INetComponent>();
        private IList<INetComponent> jtagNets = new List<INetComponent>();
        private IList<INetComponent> powerNets = new List<INetComponent>();
        private bool jtagIcTestsPerformed;
        private bool jtagPullUpTestsPerformed;
        private bool jtagPullDownTestsPerformed;
        private bool othersTestsPerformed;

        public TestCoverageBoundaryScanDeterminer(
            IEventService eventService,
            ILogger logger,
            ITestCoverageDataModel testCoverageDataModel,
            IBomUseValues bomUseValues,
            IDialogSelector dialogSelector)
        {
            this.eventService = eventService;
            this.logger = logger;
            this.dialogSelector = dialogSelector;
            this.bomUseValues = bomUseValues;
            this.testCoverageDataModel = testCoverageDataModel;
        }

        public TestCoverageBoundaryScanDeterminer(IBomUseValues bomUseValues)
        {
            this.bomUseValues = bomUseValues;
        }

        public int GetAmountOfTotalDistinctNets(
            TestCoverageType type, bool onlyIntersectionWithIcNets = true, List<IPCBComponent> jtags = null)
        {
            if (jtags == null)
            {
                jtags = this.jtags.ToList();
            }

            int result = 0;
            if (type == TestCoverageType.BOUNDARY_SCAN_ICS)
            {
                result = GetDistinctNetsFromComponents(jtags, false);
            }

            if (type == TestCoverageType.BOUNDARY_SCAN_OTHERS || type == TestCoverageType.BOUNDARY_SCAN_ALL)
            {
                List<INetComponent> otherToUse = new List<INetComponent>(GetAllNetsOfComponents(others));
                RemoveSameNetComponents(pullUps.ToList(), otherToUse);
                RemoveSameNetComponents(pullDowns.ToList(), otherToUse);
                result += GetIntersectedNets(otherToUse, jtags);
            }

            if (type == TestCoverageType.BOUNDARY_SCAN_PULL_DOWNS
                || type == TestCoverageType.BOUNDARY_SCAN_ALL
                || type == TestCoverageType.BOUNDARY_SCAN_PULL_UPS_DOWNS)
            {
                result += GetDistinctNetsFromComponents(pullDowns, onlyIntersectionWithIcNets, jtags);
            }

            if (type == TestCoverageType.BOUNDARY_SCAN_PULL_UPS
                || type == TestCoverageType.BOUNDARY_SCAN_ALL
                || type == TestCoverageType.BOUNDARY_SCAN_PULL_UPS_DOWNS)
            {
                result += GetDistinctNetsFromComponents(pullUps, onlyIntersectionWithIcNets, jtags);
            }

            return result;
        }

        private static int GetIntersectedNets(List<INetComponent> otherToUse, List<IPCBComponent> jtags)
        {
            List<INetComponent> netsToSearchFor = GetAllNetsOfComponents(jtags);
            otherToUse.RemoveAll(x => !netsToSearchFor.Contains(x));
            return otherToUse.Count;
        }

        private static void RemoveSameNetComponents(List<IPCBComponent> componentsToRemove, List<INetComponent> baseComponents)
        {
            foreach (var compToRemove in componentsToRemove)
            {
                foreach (var pinToRemove in compToRemove.Connections)
                {
                    foreach (var netToRemove in pinToRemove.Nets)
                    {
                        baseComponents.Remove(netToRemove);
                    }
                }
            }
        }

        public async Task<TestCoverageItems> GetTestCoverage(TestCoverageItems items, TestCoverageType type)
        {
            Stopwatch sw = Stopwatch.StartNew();
            TestCoverageItems result;
            if (type == TestCoverageType.BOUNDARY_SCAN_ALL)
            {
                result = await GetCompleteTestCoverageOfAllJtags(items).ConfigureAwait(false);
            }
            else if (type == TestCoverageType.BOUNDARY_SCAN_ICS)
            {
                result = await TestCoverageForIcs(items).ConfigureAwait(false);
            }
            else if (type == TestCoverageType.BOUNDARY_SCAN_OTHERS)
            {
                result = await TestCoverageForAllOthers(items).ConfigureAwait(false);
            }
            else if (type == TestCoverageType.BOUNDARY_SCAN_PULL_DOWNS)
            {
                result = await TestCoverageForPullDowns(items).ConfigureAwait(false);
            }
            else if (type == TestCoverageType.BOUNDARY_SCAN_PULL_UPS)
            {
                result = await TestCoverageForPullUps(items).ConfigureAwait(false);
            }
            else if (type == TestCoverageType.BOUNDARY_SCAN_PULL_UPS_DOWNS)
            {
                result = await TestCoverageForPullUpsDowns(items).ConfigureAwait(false);
            }
            else
            {
                logger.LogMessage("Received invalid test type!", LogCategory.ERROR);
                result = items;
            }

            sw.Stop();
            Debug.WriteLine("Test performed: " + type.ToString() + " {0}ms", sw.Elapsed.TotalMilliseconds);
            return result;
        }

        public void ClearAllObjects()
        {
            pullDowns.Clear();
            pullUps.Clear();
            others.Clear();
            jtags.Clear();
            gndNets.Clear();
            powerNets.Clear();
            jtagNets.Clear();
            testCoverageResult.Clear();
            jtagIcTestsPerformed = false;
            jtagPullDownTestsPerformed = false;
            jtagPullUpTestsPerformed = false;
            othersTestsPerformed = false;
        }

        public bool IsTestResultAvailableForComponentsAndType(IPCBComponent comp, IPCBComponent second)
        {
            if (!jtags.Contains(comp) && !jtags.Contains(second))
            {
                return false;
            }

            IPCBComponent other = second;
            if (!jtags.Contains(comp))
            {
                other = comp;
            }

            foreach (var result in testCoverageResult)
            {
                if (result.PCBComponent == other)
                {
                    if (jtags.Contains(other))
                    {
                        return jtagIcTestsPerformed;
                    }

                    if (pullDowns.Contains(other))
                    {
                        return jtagPullDownTestsPerformed;
                    }

                    if (pullUps.Contains(other))
                    {
                        return jtagPullUpTestsPerformed;
                    }

                    if (others.Contains(other))
                    {
                        return othersTestsPerformed;
                    }
                }
            }

            return false;
        }

        public ITestCoverageDataModel GetTestRelatedObjects(
            IList<INetComponent> nets,
            IdentifierBlacklistContainer content)
        {
            if (nets == null || nets.Count == 0)
            {
                return null;
            }

            var gndNets = TestCoverageBoundaryScanObjectDeterminer.GetGndNets(nets, content.GndNetIdentifier, content.GndNetBlacklist);
            var powerNets = TestCoverageBoundaryScanObjectDeterminer.GetPowerNets(nets, content.PowerNetIdentifier, content.PowerNetBlacklist);
            var jtagNets = TestCoverageBoundaryScanObjectDeterminer.GetJtagNets(nets, content.JTAGPinIdentifier, content.JTAGNetBlacklist);
            ITestCoverageDataModel dataModel = new TestCoverageDataModel();
            dataModel.Jtags = DefineJtags(jtagNets);
            dataModel.PullUps = DefinePullUpResistors(powerNets);
            dataModel.PullDowns = DefinePullDownResistors(gndNets);
            dataModel.Others = DefineOthers(dataModel.Jtags, dataModel.PullUps, dataModel.PullDowns);
            return dataModel;
        }

        public TestCoverageItems DetermineTestCoverageRelatedObjects(
            IList<INetComponent> nets,
            IdentifierBlacklistContainer content,
            TestCoverageItems items)
        {
            if (nets != null && nets.Count > 0)
            {
                DefineGndNets(nets, content.GndNetIdentifier, content.GndNetBlacklist);
                DefinePowerNets(nets, content.PowerNetIdentifier, content.PowerNetBlacklist);
                DefineJtagNets(nets, content.JTAGPinIdentifier, content.JTAGNetBlacklist);
                Dictionary<TestCoverageObject, IList<IPCBComponent>> mapObjects
                    = new Dictionary<TestCoverageObject, IList<IPCBComponent>>();
                testCoverageDataModel.Jtags = DefineJtags();
                var pullUps = DefinePullUpResistors();
                var pullDowns = DefinePullDownResistors();
                var others = DefineOthers();
                DeleteBlackListContent(pullUps, pullDowns, others);
                testCoverageDataModel.PullUps = pullUps;
                testCoverageDataModel.PullDowns = pullDowns;
                testCoverageDataModel.Others = others;
                mapObjects.Add(TestCoverageObject.PULLUP, testCoverageDataModel.PullUps);
                mapObjects.Add(TestCoverageObject.PULLDOWN, testCoverageDataModel.PullDowns);
                mapObjects.Add(TestCoverageObject.JTAG, testCoverageDataModel.Jtags);
                mapObjects.Add(TestCoverageObject.OTHERS, testCoverageDataModel.Others);
                LogTestCoverageObjects(mapObjects);
                eventService.Publish(new AddTestCoverageObjectsEvent(mapObjects));
                eventService.Publish(new TestCoverageForDeterminingObjectsPerformedEvent(TESTCOVERAGEOBJECTS.ToList()));
                items.ItemTestCoverageObjects = ITEMSELECTED;
                if (mapObjects.Count > 0)
                {
                    items.TestCoverageMenuItemsEnabled = true;
                }

                CheckDoubleMatching(testCoverageDataModel.PullDowns, testCoverageDataModel.PullUps);
                LogResults();
            }
            else
            {
                items.ItemTestCoverageObjects = ITEMNOTSELECTED;
            }

            return items;
        }

        public async Task<float> GetTestCoverageValue(TestCoverageType type, List<IPCBComponent> jtags = null)
        {
            float result = 0;
            if (type == TestCoverageType.BOUNDARY_SCAN_ALL)
            {
                float valueOfOthers = await GetTestCoveragePercentageValueForAllOtherObjects(jtags).ConfigureAwait(true);
                List<IPCBComponent> comps = new List<IPCBComponent>();
                comps.AddRange(pullUps);
                comps.AddRange(pullDowns);
                result = await GetTestCoveragePercentageValue(comps, jtags).ConfigureAwait(true) + valueOfOthers;
            }
            else if (type == TestCoverageType.BOUNDARY_SCAN_ICS)
            {
                result = await GetTestCoveragePercentageValueForIcs(jtags).ConfigureAwait(false);
            }
            else if (type == TestCoverageType.BOUNDARY_SCAN_OTHERS)
            {
                result = await GetTestCoveragePercentageValueForAllOtherObjects(jtags).ConfigureAwait(true);
            }
            else if (type == TestCoverageType.BOUNDARY_SCAN_PULL_DOWNS)
            {
                result = await GetTestCoveragePercentageValue(pullDowns, jtags).ConfigureAwait(false);
            }
            else if (type == TestCoverageType.BOUNDARY_SCAN_PULL_UPS)
            {
                result = await GetTestCoveragePercentageValue(pullUps, jtags).ConfigureAwait(false);
            }
            else if (type == TestCoverageType.BOUNDARY_SCAN_PULL_UPS_DOWNS)
            {
                List<IPCBComponent> comps = new List<IPCBComponent>();
                comps.AddRange(pullUps);
                comps.AddRange(pullDowns);
                result = await GetTestCoveragePercentageValue(comps, jtags).ConfigureAwait(false);
            }
            else
            {
                logger.LogMessage("Received invalid test type!", LogCategory.ERROR);
            }

            return result;
        }

        private static string ToggleItem(string value)
        {
            if (value.Equals(ITEMSELECTED))
            {
                return ITEMNOTSELECTED;
            }
            else
            {
                return ITEMSELECTED;
            }
        }

        private static List<INetComponent> GetAllNetsOfComponents(IList<IPCBComponent> pullUpsDowns)
        {
            HashSet<INetComponent> nets = new HashSet<INetComponent>();
            foreach (var comp in pullUpsDowns)
            {
                foreach (var pin in comp.Connections)
                {
                    foreach (var net in pin.Nets)
                    {
                        if (!nets.Contains(net))
                        {
                            nets.Add(net);
                        }
                    }
                }
            }

            return nets.ToList();
        }

        private static string GetNetsOfComp(IList<IPinComponent> connections)
        {
            StringBuilder sb = new StringBuilder(", with nets: ");
            List<INetComponent> nets = new List<INetComponent>();
            foreach (var pin in connections)
            {
                foreach (var net in pin.Nets)
                {
                    if (!nets.Contains(net))
                    {
                        nets.Add(net);
                        sb.Append(net.NetName + ", ");
                    }
                }
            }

            return sb.ToString()[0..^2];
        }

        private static bool GotTheSameNet(IList<INetComponent> nets1, IList<INetComponent> nets2)
        {
            foreach (var netInner in nets1)
            {
                foreach (var netOuter in nets2)
                {
                    if (netInner == netOuter)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private void DeleteBlackListContent(IList<IPCBComponent> pullUps, IList<IPCBComponent> pullDowns, IList<IPCBComponent> others)
        {
            // TO DO: IMPLEMENT LOGIC FOR DETERMINING ALL OBJECTS WHICH SHOULD BE DELETED FROM SVF FILE GENERATION
            dialogSelector.ToString();
        }

        private int GetDistinctNetsFromComponents(
            IList<IPCBComponent> components, bool onlyWhichAreInIcs = true, List<IPCBComponent> jtags = null)
        {
            if (jtags == null)
            {
                jtags = this.jtags.ToList();
            }

            HashSet<INetComponent> nets = new HashSet<INetComponent>();
            foreach (var comp in components)
            {
                foreach (var pin in comp.Connections)
                {
                    foreach (var net in pin.Nets)
                    {
                        nets.Add(net);
                    }
                }
            }

            if (onlyWhichAreInIcs)
            {
                List<INetComponent> netsIcs = GetAllNetsOfComponents(jtags);
                List<INetComponent> netsFound = new List<INetComponent>();
                foreach (var net in nets)
                {
                    if (netsIcs.Contains(net))
                    {
                        netsFound.Add(net);
                    }
                }

                return netsFound.Count;
            }
            else
            {
                return nets.Count;
            }
        }

        private async Task<TestCoverageItems> TestCoverageForIcs(TestCoverageItems items)
        {
            bool value = false;
            IList<ITestCoverageResult> result = DefineTestCoverageForIcObjects();
            if (result.Count > 0)
            {
                await eventService.Publish(new RefreshTestcoverageResultObjectsEvent()).ConfigureAwait(false);
                items.ItemTestCoverageJtag = ToggleItem(items.ItemTestCoverageJtag);
                value = !GetTestsPerformedState(TestCoverageObject.JTAG);
            }
            else
            {
                logger.LogMessage("No ICs were detected before", LogCategory.WARNING);
                items.ItemTestCoverageJtag = ITEMNOTSELECTED;
            }

            SetTestsPerformedState(TestCoverageObject.JTAG, value);
            items.TestCoverage = await GetTestCoveragePercentageValueForIcs().ConfigureAwait(false);
            items.TestCoverageVisibilty = true;
            return items;
        }

        private async Task<TestCoverageItems> TestCoverageForPullUps(TestCoverageItems items)
        {
            bool value = false;
            IList<ITestCoverageResult> results = await DefineTestCoverageForPullUpDownObjects(DefineJtags(), DefinePullUpResistors()).ConfigureAwait(true);
            if (results.Count > 0)
            {
                await eventService.Publish(new RefreshTestcoverageResultObjectsEvent()).ConfigureAwait(false);
                items.ItemTestCoveragePullup = ToggleItem(items.ItemTestCoveragePullup);
                value = !GetTestsPerformedState(TestCoverageObject.PULLUP);
            }
            else
            {
                logger.LogMessage("No pull ups were detected before", LogCategory.WARNING);
                items.ItemTestCoveragePullup = ITEMNOTSELECTED;
            }

            SetTestsPerformedState(TestCoverageObject.PULLUP, value);
            items.TestCoverage = await GetTestCoveragePercentageValue(DefinePullUpResistors()).ConfigureAwait(false);
            items.TestCoverageVisibilty = true;
            return items;
        }

        private async Task<TestCoverageItems> TestCoverageForPullDowns(TestCoverageItems items)
        {
            bool value = false;
            IList<ITestCoverageResult> results = await DefineTestCoverageForPullUpDownObjects(DefineJtags(), DefinePullDownResistors()).ConfigureAwait(true);
            if (results.Count > 0)
            {
                await eventService.Publish(new RefreshTestcoverageResultObjectsEvent()).ConfigureAwait(false);
                items.ItemTestCoveragePulldown = ToggleItem(items.ItemTestCoveragePulldown);
                value = !GetTestsPerformedState(TestCoverageObject.PULLDOWN);
            }
            else
            {
                logger.LogMessage("No pull downs were detected before", LogCategory.WARNING);
                items.ItemTestCoveragePulldown = ITEMNOTSELECTED;
            }

            SetTestsPerformedState(TestCoverageObject.PULLDOWN, value);
            items.TestCoverage = await GetTestCoveragePercentageValue(DefinePullDownResistors()).ConfigureAwait(false);
            items.TestCoverageVisibilty = true;
            return items;
        }

        private async Task<TestCoverageItems> TestCoverageForPullUpsDowns(TestCoverageItems items)
        {
            bool value = false;
            IList<ITestCoverageResult> results = await DefineTestCoverageForPullUpDownObjects().ConfigureAwait(true);
            if (results.Count > 0)
            {
                await eventService.Publish(new RefreshTestcoverageResultObjectsEvent()).ConfigureAwait(false);
                items.ItemTestCoveragePullupdown = ToggleItem(items.ItemTestCoveragePullupdown);
                if (items.ItemTestCoveragePullupdown.Equals(ITEMSELECTED))
                {
                    items.ItemTestCoveragePulldown = ITEMSELECTED;
                    items.ItemTestCoveragePullup = ITEMSELECTED;
                    value = true;
                }
                else
                {
                    items.ItemTestCoveragePulldown = ITEMNOTSELECTED;
                    items.ItemTestCoveragePullup = ITEMNOTSELECTED;
                }
            }
            else
            {
                logger.LogMessage("No pull ups and pull downs were detected before", LogCategory.WARNING);
                items.ItemTestCoveragePulldown = ITEMNOTSELECTED;
                items.ItemTestCoveragePullup = ITEMNOTSELECTED;
                items.ItemTestCoveragePullupdown = ITEMNOTSELECTED;
            }

            SetTestsPerformedState(TestCoverageObject.PULLUP, value);
            SetTestsPerformedState(TestCoverageObject.PULLDOWN, value);
            items.TestCoverage = await GetTestCoveragePercentageValue().ConfigureAwait(false);
            items.TestCoverageVisibilty = true;
            return items;
        }

        private async Task<TestCoverageItems> TestCoverageForAllOthers(TestCoverageItems items)
        {
            bool valueBool = false;
            float value = await GetTestCoveragePercentageValueForAllOtherObjects().ConfigureAwait(true);
            items.TestCoverageVisibilty = true;
            items.TestCoverage = value;
            if (value > 0)
            {
                items.ItemTestCoverageOthers = ToggleItem(items.ItemTestCoverageOthers);
                valueBool = !GetTestsPerformedState(TestCoverageObject.OTHERS);
            }
            else
            {
                logger.LogMessage("No other components connected to ICs were detected before", LogCategory.WARNING);
                items.ItemTestCoverageOthers = ITEMNOTSELECTED;
            }

            SetTestsPerformedState(TestCoverageObject.OTHERS, valueBool);
            return items;
        }

        private async Task<TestCoverageItems> GetCompleteTestCoverageOfAllJtags(TestCoverageItems items)
        {
            bool value = false;
            bool someThingChanged = false;
            IList<ITestCoverageResult> listPullUpsPullDowns = await DefineTestCoverageForPullUpDownObjects().ConfigureAwait(true);
            if (listPullUpsPullDowns != null && listPullUpsPullDowns.Count > 0)
            {
                someThingChanged = true;
            }

            IList<ITestCoverageResult> listJtags = DefineTestCoverageForIcObjects();
            if (listJtags != null && listJtags.Count > 0)
            {
                someThingChanged = true;
            }

            float valueOfOthers = await GetTestCoveragePercentageValueForAllOtherObjects().ConfigureAwait(true);
            if (valueOfOthers > 0)
            {
                someThingChanged = true;
            }

            items.TestCoverage = await GetTestCoveragePercentageValue().ConfigureAwait(true) + valueOfOthers;
            if (items.TestCoverage > 100)
            {
                items.TestCoverage = 100;
                logger.LogMessage("Test coverage is greater than 100%! Please check your settings", LogCategory.WARNING);
            }

            items.TestCoverageVisibilty = true;
            string textValue = ITEMNOTSELECTED;
            if (someThingChanged)
            {
                await eventService.Publish(new RefreshTestcoverageResultObjectsEvent()).ConfigureAwait(false);
                items.ItemTestCoverageAll = ToggleItem(items.ItemTestCoverageAll);
                if (items.ItemTestCoverageAll.Equals(ITEMSELECTED))
                {
                    textValue = ITEMSELECTED;
                    value = true;
                }
            }

            items.ItemTestCoveragePulldown = textValue;
            items.ItemTestCoveragePullup = textValue;
            items.ItemTestCoveragePullupdown = textValue;
            items.ItemTestCoverageJtag = textValue;
            items.ItemTestCoverageAll = textValue;
            items.ItemTestCoverageOthers = textValue;

            SetTestsPerformedState(TestCoverageObject.PULLUP, value);
            SetTestsPerformedState(TestCoverageObject.PULLDOWN, value);
            SetTestsPerformedState(TestCoverageObject.JTAG, value);
            SetTestsPerformedState(TestCoverageObject.OTHERS, value);
            return items;
        }

        private IList<ITestCoverageResult> DefineTestCoverageForIcObjects(IList<IPCBComponent> ics = null)
        {
            if (ics == null)
            {
                ics = jtags;
            }

            IList<ITestCoverageResult> coverageResult = new List<ITestCoverageResult>();
            if (ics == null || ics.Count == 0)
            {
                return coverageResult;
            }

            foreach (var pcbObject in ics)
            {
                foreach (var pin in pcbObject.Connections)
                {
                    foreach (var secondObj in ics)
                    {
                        if (pcbObject != secondObj)
                        {
                            foreach (var secondPin in secondObj.Connections)
                            {
                                if (GotTheSameNet(pin.Nets, secondPin.Nets))
                                {
                                    if (!IsTestResultAvailableForComponent(pcbObject, coverageResult))
                                    {
                                        coverageResult.Add(new TestCoverageResult(pcbObject, 100.0f));
                                    }

                                    if (!IsTestResultAvailableForComponent(secondObj, coverageResult))
                                    {
                                        coverageResult.Add(new TestCoverageResult(secondObj, 100.0f));
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }
            }

            FinalizeTestCoverageDetermination(coverageResult);
            return coverageResult;
        }

        private void SetTestsPerformedState(TestCoverageObject objectType, bool value)
        {
            if (objectType == TestCoverageObject.JTAG)
            {
                jtagIcTestsPerformed = value;
            }
            else if (objectType == TestCoverageObject.PULLDOWN)
            {
                jtagPullDownTestsPerformed = value;
            }
            else if (objectType == TestCoverageObject.PULLUP)
            {
                jtagPullUpTestsPerformed = value;
            }
            else
            {
                othersTestsPerformed = value;
            }
        }

        private bool GetTestsPerformedState(TestCoverageObject objectType)
        {
            if (objectType == TestCoverageObject.JTAG)
            {
                return jtagIcTestsPerformed;
            }
            else if (objectType == TestCoverageObject.PULLDOWN)
            {
                return jtagPullDownTestsPerformed;
            }
            else if (objectType == TestCoverageObject.PULLUP)
            {
                return jtagPullUpTestsPerformed;
            }
            else
            {
                return othersTestsPerformed;
            }
        }

        private IList<IPCBComponent> DefineOthers(
            IList<IPCBComponent> jtags = null,
            IList<IPCBComponent> pullDowns = null,
            IList<IPCBComponent> pullUps = null)
        {
            IList<IPCBComponent> jtagToUse = jtags;
            if (jtagToUse == null)
            {
                jtagToUse = this.jtags;
            }

            IList<IPCBComponent> pullDownToUse = pullDowns;
            if (pullDownToUse == null)
            {
                pullDownToUse = this.pullDowns;
            }

            IList<IPCBComponent> pullUpToUse = pullUps;
            if (pullUpToUse == null)
            {
                pullUpToUse = this.pullUps;
            }

            others = TestCoverageBoundaryScanObjectDeterminer.GetOthers(jtagToUse, pullDownToUse, pullUpToUse);
            var num = ((List<IPCBComponent>)others).RemoveAll(x => string.IsNullOrEmpty(x.FunctionalAttributes.Value) && bomUseValues.UseValues);

            return others;
        }

        private IList<INetComponent> DefineGndNets(IList<INetComponent> nets, string gndIdentifier, string gndBlacklist)
        {
            gndNets = TestCoverageBoundaryScanObjectDeterminer.GetGndNets(nets, gndIdentifier, gndBlacklist);
            return gndNets;
        }

        private IList<IPCBComponent> DefineJtags(IList<INetComponent> jtagNets = null)
        {
            if (jtagNets == null)
            {
                jtagNets = this.jtagNets;
            }

            jtags = TestCoverageBoundaryScanObjectDeterminer.GetIcs(jtagNets);
            var num = ((List<IPCBComponent>)jtags).RemoveAll(x => string.IsNullOrEmpty(x.FunctionalAttributes.Value) && bomUseValues.UseValues);

            return jtags;
        }

        private IList<INetComponent> DefineJtagNets(IList<INetComponent> nets, string jtagIdentifier, string jtagBlacklist)
        {
            jtagNets = TestCoverageBoundaryScanObjectDeterminer.GetJtagNets(nets, jtagIdentifier, jtagBlacklist);
            return jtagNets;
        }

        private IList<INetComponent> DefinePowerNets(IList<INetComponent> nets, string powerIdentifier, string powerBlacklist)
        {
            powerNets = TestCoverageBoundaryScanObjectDeterminer.GetPowerNets(nets, powerIdentifier, powerBlacklist);
            return powerNets;
        }

        private IList<IPCBComponent> DefinePullDownResistors(IList<INetComponent> net = null)
        {
            if (net == null)
            {
                net = gndNets;
            }

            pullDowns = TestCoverageBoundaryScanObjectDeterminer.GetPullUpDownResistors(net);
            var num = ((List<IPCBComponent>)pullDowns).RemoveAll(x => string.IsNullOrEmpty(x.FunctionalAttributes.Value) && bomUseValues.UseValues);

            return pullDowns;
        }

        private IList<IPCBComponent> DefinePullUpResistors(IList<INetComponent> powerNets = null)
        {
            if (powerNets == null)
            {
                powerNets = this.powerNets;
            }

            pullUps = TestCoverageBoundaryScanObjectDeterminer.GetPullUpDownResistors(powerNets);
            var num = ((List<IPCBComponent>)pullUps).RemoveAll(x => string.IsNullOrEmpty(x.FunctionalAttributes.Value) && bomUseValues.UseValues);

            return pullUps;
        }

        private async Task<float> GetTestCoveragePercentageValue(
            IList<IPCBComponent> pullUpsDowns = null, IList<IPCBComponent> ics = null)
        {
            if (pullUpsDowns == null)
            {
                await Task.Run(() =>
                {
                    pullUpsDowns = new List<IPCBComponent>();
                    foreach (var comp in pullUps)
                    {
                        pullUpsDowns.Add(comp);
                    }

                    foreach (var comp in pullDowns)
                    {
                        pullUpsDowns.Add(comp);
                    }
                }).ConfigureAwait(true);
            }

            if (pullUpsDowns.Count == 0)
            {
                return 0;
            }

            if (ics == null)
            {
                ics = jtags;
            }

            int baseValue = 0;
            await Task.Run(() =>
            {
                foreach (var ic in ics)
                {
                    baseValue += ic.Connections.Count;
                }
            }).ConfigureAwait(true);

            if (baseValue == 0)
            {
                return 0;
            }

            List<INetComponent> netsOfPullUpsPullDowns = GetAllNetsOfComponents(pullUpsDowns);
            List<INetComponent> netsOfIcs = GetAllNetsOfComponents(ics);
            netsOfPullUpsPullDowns.RemoveAll(x => !netsOfIcs.Contains(x));
            logger.LogMessage(
                "Test coverage calculation for " + netsOfPullUpsPullDowns.Count + " / " + baseValue + " * 100",
                LogCategory.INFO);
            return (float)netsOfPullUpsPullDowns.Count / baseValue * 100;
        }

        private async Task<float> GetTestCoveragePercentageValueForAllOtherObjects(IList<IPCBComponent> ics = null)
        {
            if (ics == null)
            {
                ics = jtags;
            }

            int baseNumber = 0;
            foreach (var comp in ics)
            {
                baseNumber += comp.Connections.Count;
            }

            if (baseNumber == 0)
            {
                return 0;
            }

            int amountNets = 0;
            await Task.Run(() =>
            {
                IList<INetComponent> nets = new List<INetComponent>();
                IList<INetComponent> netsFromIc = GetAllNetsOfComponents(ics);

                foreach (var comp in others)
                {
                    foreach (var pin in comp.Connections)
                    {
                        foreach (var net in pin.Nets)
                        {
                            IList<INetComponent> netToCheck = new List<INetComponent>
                            {
                                net,
                            };
                            if (!nets.Contains(net) && GotTheSameNet(netToCheck, netsFromIc))
                            {
                                nets.Add(net);
                            }
                        }
                    }
                }

                var newNets = nets.ToList();
                List<INetComponent> netsOfPullUps = GetAllNetsOfComponents(pullUps);
                netsOfPullUps.RemoveAll(x => !netsFromIc.Contains(x));
                newNets.RemoveAll(x => netsOfPullUps.Contains(x));
                List<INetComponent> netsOfPullDowns = GetAllNetsOfComponents(pullDowns);
                netsOfPullDowns.RemoveAll(x => !netsFromIc.Contains(x));
                newNets.RemoveAll(x => netsOfPullDowns.Contains(x));

                amountNets = newNets.Count;
                IList<ITestCoverageResult> results = new List<ITestCoverageResult>();
                foreach (var comp in others)
                {
                    results.Add(new TestCoverageResult(comp, 100.0f));
                }

                FinalizeTestCoverageDetermination(results);
            }).ConfigureAwait(true);

            logger.LogMessage("Test coverage calculation for " + amountNets + " / " + baseNumber + " * 100", LogCategory.INFO);
            return (float)amountNets / baseNumber * 100;
        }

        private async Task<IList<ITestCoverageResult>> DefineTestCoverageForPullUpDownObjects(
            IList<IPCBComponent> ics = null, IList<IPCBComponent> pullUpsDowns = null)
        {
            IList<ITestCoverageResult> results = new List<ITestCoverageResult>();
            await Task.Run(() =>
            {
                if (ics == null)
                {
                    ics = jtags;
                }

                IList<IPCBComponent> pulls = pullUpsDowns;
                if (pulls == null)
                {
                    pulls = new List<IPCBComponent>();
                    foreach (var comp in pullDowns)
                    {
                        pulls.Add(comp);
                    }

                    foreach (var comp in pullUps)
                    {
                        pulls.Add(comp);
                    }
                }

                foreach (var ic in ics)
                {
                    foreach (var pin in ic.Connections)
                    {
                        foreach (var comp in pulls)
                        {
                            foreach (var net in comp.Connections)
                            {
                                if (GotTheSameNet(pin.Nets, net.Nets))
                                {
                                    if (!IsTestResultAvailableForComponent(ic, results))
                                    {
                                        results.Add(new TestCoverageResult(ic, 100.0f));
                                    }

                                    if (!IsTestResultAvailableForComponent(comp, results))
                                    {
                                        results.Add(new TestCoverageResult(comp, 100.0f));
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }

                FinalizeTestCoverageDetermination(results);
            }).ConfigureAwait(false);

            return results;
        }

        private async Task<float> GetTestCoveragePercentageValueForIcs(IList<IPCBComponent> ics = null)
        {
            if (ics == null)
            {
                ics = jtags;
            }

            if (ics == null || ics.Count == 0)
            {
                return 0;
            }

            int baseValue = 0;
            await Task.Run(() =>
            {
                foreach (var ic in ics)
                {
                    baseValue += ic.Connections.Count;
                }
            }).ConfigureAwait(true);

            if (baseValue == 0)
            {
                return 0;
            }

            logger.LogMessage("Test coverage calculation for " + ics.Count + " / " + baseValue + " * 100", LogCategory.INFO);
            return (float)ics.Count / baseValue * 100;
        }

        private void LogResults()
        {
            List<INetComponent> netsOfIc = GetAllNetsOfComponents(jtags);
            List<INetComponent> netsOfOther = GetAllNetsOfComponents(others);
            List<INetComponent> netsOfPullups = GetAllNetsOfComponents(pullUps);
            List<INetComponent> netsOfPullDowns = GetAllNetsOfComponents(pullDowns);
            netsOfOther.RemoveAll(x => !netsOfIc.Contains(x));
            netsOfPullups.RemoveAll(x => !netsOfIc.Contains(x));
            netsOfPullDowns.RemoveAll(x => !netsOfIc.Contains(x));
            logger.LogMessage("Totally detected amount of nets of ICs: " + netsOfIc.Count, LogCategory.INFO);
            logger.LogMessage("Totally detected amount of same nets of PullUps: " + netsOfPullups.Count, LogCategory.INFO);
            logger.LogMessage("Totally detected amount of same nets of PullDowns: " + netsOfPullDowns.Count, LogCategory.INFO);
            logger.LogMessage("Totally detected amount of same nets of Others: " + netsOfOther.Count, LogCategory.INFO);
        }

        private void CheckDoubleMatching(IList<IPCBComponent> pullDowns, IList<IPCBComponent> pullUps)
        {
            bool foundDoubles = false;
            List<IPCBComponent> comps = new List<IPCBComponent>();
            foreach (var comp in pullUps)
            {
                if (pullDowns.Contains(comp) && !comps.Contains(comp))
                {
                    if (!foundDoubles)
                    {
                        foundDoubles = true;
                        logger.LogMessage(ErrorMessageDoublesFound, LogCategory.WARNING);
                    }

                    comps.Add(comp);
                    logger.LogMessage(
                        comp.FunctionalAttributes.Ref + ":" + comp.FunctionalAttributes.Value
                        + GetNetsOfComp(comp.Connections),
                        LogCategory.WARNING);
                }
            }
        }

        private void LogTestCoverageObjects(Dictionary<TestCoverageObject, IList<IPCBComponent>> mapObjects)
        {
            logger.LogMessage("Determined pull up objects: " + mapObjects[TestCoverageObject.PULLUP].Count, LogCategory.INFO);
            logger.LogMessage("Determined pull down objects: " + mapObjects[TestCoverageObject.PULLDOWN].Count, LogCategory.INFO);
            logger.LogMessage("Determined IC objects: " + mapObjects[TestCoverageObject.JTAG].Count, LogCategory.INFO);
            logger.LogMessage(
                "Determined other objects which are connected to the ICs: "
                + mapObjects[TestCoverageObject.OTHERS].Count,
                LogCategory.INFO);
        }

        private bool IsTestResultAvailableForComponent(IPCBComponent comp, IList<ITestCoverageResult> coverageResult = null)
        {
            if (coverageResult == null)
            {
                coverageResult = testCoverageResult;
            }

            return coverageResult.FirstOrDefault(result => result.PCBComponent == comp) != null;
        }

        private void FinalizeTestCoverageDetermination(IList<ITestCoverageResult> results)
        {
            foreach (var result in results)
            {
                if (!testCoverageResult.Contains(result)
                    && !IsTestResultAvailableForComponent(result.PCBComponent, testCoverageResult))
                {
                    testCoverageResult.Add(result);
                }
            }
        }
    }
}
