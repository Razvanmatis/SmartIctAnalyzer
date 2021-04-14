using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces.Gui;
using Interfaces.PcbInvestigator;
using ProMik.Core.Interfaces.Events;
using TestCoverage.Events;
using TestCoverage.Helper;
using TestCoverage.Interfaces;

namespace TestCoverage.Implementations
{
    public class TestCoverageBoundaryScanDeterminer : ITestCoverageDeterminer
    {
        public const string ITEMNOTSELECTED = "CheckboxBlankOutline";
        public const string ErrorMessageDoublesFound = "Detected the following resistors as PULLUPS and PULLDOWNS! Please check your settings: ";
        public const string ITEMSELECTED = "Checkbox";
        private static readonly string[] TESTCOVERAGEOBJECTS = new string[] { "Pullups", "Pulldowns", "JTAG", "Others" };
        private readonly IList<ITestCoverageResult> testCoverageResult = new List<ITestCoverageResult>();
        private readonly IEventService eventService;
        private readonly ILogger logger;
        private readonly ITestCoverageDataModel testCoverageDataModel;
        private IList<IPCBComponent> pullDowns = new List<IPCBComponent>();
        private IList<IPCBComponent> pullUps = new List<IPCBComponent>();
        private IList<IPCBComponent> others = new List<IPCBComponent>();
        private IList<IPCBComponent> ics = new List<IPCBComponent>();
        private IList<INetComponent> gndNets = new List<INetComponent>();
        private IList<INetComponent> jtagNets = new List<INetComponent>();
        private IList<INetComponent> powerNets = new List<INetComponent>();
        private bool jtagIcTestsPerformed;
        private bool jtagPullUpTestsPerformed;
        private bool jtagPullDownTestsPerformed;
        private bool othersTestsPerformed;

        public TestCoverageBoundaryScanDeterminer(IEventService eventService, ILogger logger, ITestCoverageDataModel testCoverageDataModel)
        {
            this.eventService = eventService;
            this.logger = logger;
            this.testCoverageDataModel = testCoverageDataModel;
        }

        public async Task<TestCoverageItems> GetTestCoverage(TestCoverageItems items, TestCoverageType type)
        {
            Stopwatch sw = Stopwatch.StartNew();
            TestCoverageItems result = null;
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
            ics.Clear();
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
            if (!this.ics.Contains(comp) && !this.ics.Contains(second))
            {
                return false;
            }

            IPCBComponent other = second;
            if (!this.ics.Contains(comp))
            {
                other = comp;
            }

            foreach (var result in testCoverageResult)
            {
                if (result.PCBComponent == other)
                {
                    if (ics.Contains(other))
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

        public TestCoverageItems DetermineTestCoverageRelatedObjects(
            IList<INetComponent> nets,
            IdentifierBlacklistContainer content,
            TestCoverageItems items)
        {
            if (nets != null && nets.Count > 0)
            {
                DefineGndNets(nets, content.GndNetIdentifier, content.GndNetBlacklist);
                DefinePowerNets(nets, content.PowerNetIdentifier, content.PowerNetBlacklist);
                DefineJtagNets(nets, content.JTAGNetIdentifier, content.JTAGNetBlacklist);
                Dictionary<TestCoverageObject, IList<IPCBComponent>> mapObjects = new Dictionary<TestCoverageObject, IList<IPCBComponent>>();
                testCoverageDataModel.Ics = DefineIcs();
                testCoverageDataModel.PullUps = DefinePullUpResistors();
                testCoverageDataModel.PullDowns = DefinePullDownResistors();
                mapObjects.Add(TestCoverageObject.PULLUP, testCoverageDataModel.PullUps);
                mapObjects.Add(TestCoverageObject.PULLDOWN, testCoverageDataModel.PullDowns);
                mapObjects.Add(TestCoverageObject.JTAG, testCoverageDataModel.Ics);
                mapObjects.Add(TestCoverageObject.OTHERS, DefineOthers());
                LogTestCoverageObjects(mapObjects);
                eventService.Publish(new AddTestCoverageObjectsEvent(mapObjects));
                eventService.Publish(new TestCoverageForDeterminingObjectsPerformedEvent(TESTCOVERAGEOBJECTS.ToList()));
                items.ItemTestCoverageObjects = TestCoverageBoundaryScanDeterminer.ITEMSELECTED;
                if (mapObjects.Count > 0)
                {
                    items.TestCoverageMenuItemsEnabled = true;
                }

                CheckDoubleMatching(testCoverageDataModel.PullDowns, testCoverageDataModel.PullUps);
                LogResults();
            }
            else
            {
                items.ItemTestCoverageObjects = TestCoverageBoundaryScanDeterminer.ITEMNOTSELECTED;
            }

            return items;
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
            List<INetComponent> nets = new List<INetComponent>();
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

            return nets;
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

        private async Task<TestCoverageItems> TestCoverageForIcs(TestCoverageItems items)
        {
            bool value = false;
            IList<ITestCoverageResult> result = DefineTestCoverageForIcObjects();
            if (result.Count > 0)
            {
                eventService.Publish<RefreshTestcoverageResultObjectsEvent>(new RefreshTestcoverageResultObjectsEvent());
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
            IList<ITestCoverageResult> results = await DefineTestCoverageForPullUpDownObjects(DefineIcs(), DefinePullUpResistors()).ConfigureAwait(true);
            if (results.Count > 0)
            {
                eventService.Publish<RefreshTestcoverageResultObjectsEvent>(new RefreshTestcoverageResultObjectsEvent());
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
            IList<ITestCoverageResult> results = await DefineTestCoverageForPullUpDownObjects(DefineIcs(), DefinePullDownResistors()).ConfigureAwait(true);
            if (results.Count > 0)
            {
                eventService.Publish<RefreshTestcoverageResultObjectsEvent>(new RefreshTestcoverageResultObjectsEvent());
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
                eventService.Publish<RefreshTestcoverageResultObjectsEvent>(new RefreshTestcoverageResultObjectsEvent());
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
                eventService.Publish<RefreshTestcoverageResultObjectsEvent>(new RefreshTestcoverageResultObjectsEvent());
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
                ics = this.ics;
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

        private IList<IPCBComponent> DefineOthers()
        {
            others = TestCoverageBoundaryScanObjectDeterminer.GetOthers(ics, pullDowns, pullUps);
            return others;
        }

        private IList<INetComponent> DefineGndNets(IList<INetComponent> nets, string gndIdentifier, string gndBlacklist)
        {
            gndNets = TestCoverageBoundaryScanObjectDeterminer.GetGndNets(nets, gndIdentifier, gndBlacklist);
            return gndNets;
        }

        private IList<IPCBComponent> DefineIcs(IList<INetComponent> jtagNets = null)
        {
            if (jtagNets == null)
            {
                jtagNets = this.jtagNets;
            }

            ics = TestCoverageBoundaryScanObjectDeterminer.GetIcs(jtagNets);
            return ics;
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
                net = this.gndNets;
            }

            pullDowns = TestCoverageBoundaryScanObjectDeterminer.GetPullUpDownResistors(net);
            return pullDowns;
        }

        private IList<IPCBComponent> DefinePullUpResistors(IList<INetComponent> powerNets = null)
        {
            if (powerNets == null)
            {
                powerNets = this.powerNets;
            }

            pullUps = TestCoverageBoundaryScanObjectDeterminer.GetPullUpDownResistors(powerNets);
            return pullUps;
        }

        private async Task<float> GetTestCoveragePercentageValue(IList<IPCBComponent> pullUpsDowns = null, IList<IPCBComponent> ics = null)
        {
            if (pullUpsDowns == null)
            {
                await Task.Run(() =>
                {
                    pullUpsDowns = new List<IPCBComponent>();
                    foreach (var comp in this.pullUps)
                    {
                        pullUpsDowns.Add(comp);
                    }

                    foreach (var comp in this.pullDowns)
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
                ics = this.ics;
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
            List<INetComponent> netsOfIcs = GetAllNetsOfComponents(this.ics);
            netsOfPullUpsPullDowns.RemoveAll(x => !netsOfIcs.Contains(x));
            logger.LogMessage("Test coverage calculation for " + netsOfPullUpsPullDowns.Count + " / " + baseValue + " * 100", LogCategory.INFO);
            return (float)netsOfPullUpsPullDowns.Count / baseValue * 100;
        }

        private async Task<float> GetTestCoveragePercentageValueForAllOtherObjects(IList<IPCBComponent> ics = null)
        {
            if (ics == null)
            {
                ics = this.ics;
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
                IList<INetComponent> netsFromIc = GetAllNetsOfComponents(this.ics);

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

                amountNets = nets.Count;

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

        private async Task<IList<ITestCoverageResult>> DefineTestCoverageForPullUpDownObjects(IList<IPCBComponent> ics = null, IList<IPCBComponent> pullUpsDowns = null)
        {
            IList<ITestCoverageResult> results = new List<ITestCoverageResult>();
            await Task.Run(() =>
            {
                if (ics == null)
                {
                    ics = this.ics;
                }

                IList<IPCBComponent> pulls = pullUpsDowns;
                if (pulls == null)
                {
                    pulls = new List<IPCBComponent>();
                    foreach (var comp in this.pullDowns)
                    {
                        pulls.Add(comp);
                    }

                    foreach (var comp in this.pullUps)
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
                ics = this.ics;
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
            List<INetComponent> netsOfIc = GetAllNetsOfComponents(this.ics);
            List<INetComponent> netsOfOther = GetAllNetsOfComponents(this.others);
            List<INetComponent> netsOfPullups = GetAllNetsOfComponents(this.pullUps);
            List<INetComponent> netsOfPullDowns = GetAllNetsOfComponents(this.pullDowns);
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
                    logger.LogMessage(comp.FunctionalAttributes.Ref + ":" + comp.FunctionalAttributes.Value + GetNetsOfComp(comp.Connections), LogCategory.WARNING);
                }
            }
        }

        private void LogTestCoverageObjects(Dictionary<TestCoverageObject, IList<IPCBComponent>> mapObjects)
        {
            logger.LogMessage("Determined pull up objects: " + mapObjects[TestCoverageObject.PULLUP].Count, LogCategory.INFO);
            logger.LogMessage("Determined pull down objects: " + mapObjects[TestCoverageObject.PULLDOWN].Count, LogCategory.INFO);
            logger.LogMessage("Determined IC objects: " + mapObjects[TestCoverageObject.JTAG].Count, LogCategory.INFO);
            logger.LogMessage("Determined other objects which are connected to the ICs: " + mapObjects[TestCoverageObject.OTHERS].Count, LogCategory.INFO);
        }

        private bool IsTestResultAvailableForComponent(IPCBComponent comp, IList<ITestCoverageResult> coverageResult = null)
        {
            if (coverageResult == null)
            {
                coverageResult = testCoverageResult;
            }

            foreach (var result in coverageResult)
            {
                if (result.PCBComponent == comp)
                {
                    return true;
                }
            }

            return false;
        }

        private void FinalizeTestCoverageDetermination(IList<ITestCoverageResult> results)
        {
            foreach (var result in results)
            {
                if (!testCoverageResult.Contains(result) && !IsTestResultAvailableForComponent(result.PCBComponent, testCoverageResult))
                {
                    testCoverageResult.Add(result);
                }
            }
        }
    }
}
