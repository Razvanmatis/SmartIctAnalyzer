using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Interfaces;
using Interfaces.PcbInvestigator;
using Interfaces.TestCoverage;

namespace TestCoverage
{
    public class TestCoverageDeterminer : ITestCoverageDeterminer
    {
        private IList<IPCBComponent> pullDowns = new List<IPCBComponent>();
        private IList<IPCBComponent> pullUps = new List<IPCBComponent>();
        private IList<IPCBComponent> ics = new List<IPCBComponent>();
        private IList<INetComponent> gndNets = new List<INetComponent>();
        private IList<INetComponent> jtagNets = new List<INetComponent>();
        private IList<INetComponent> powerNets = new List<INetComponent>();
        private IList<ITestCoverageResult> testCoverageResult = new List<ITestCoverageResult>();
        private bool jtagIcTestsPerformed;
        private bool jtagPullUpTestsPerformed;
        private bool jtagPullDownTestsPerformed;

        public IList<ITestCoverageResult> DefineTestCoverageForIcObjects(IList<IPCBComponent> ics = null)
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
            jtagIcTestsPerformed = !jtagIcTestsPerformed;
            return coverageResult;
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
                }
            }

            return false;
        }

        public void ClearAllObjects()
        {
            pullDowns.Clear();
            pullUps.Clear();
            ics.Clear();
            gndNets.Clear();
            powerNets.Clear();
            jtagNets.Clear();
            testCoverageResult.Clear();
        }

        public IList<INetComponent> DefineGndNets(IList<INetComponent> nets, string gndIdentifier, string gndBlacklist)
        {
            gndNets = TestCoverageObjectDeterminer.GetGndNets(nets, gndIdentifier, gndBlacklist);
            return gndNets;
        }

        public IList<IPCBComponent> DefineIcs(IList<INetComponent> jtagNets = null)
        {
            if (jtagNets == null)
            {
                jtagNets = this.jtagNets;
            }

            ics = TestCoverageObjectDeterminer.GetIcs(jtagNets);
            return ics;
        }

        public IList<INetComponent> DefineJtagNets(IList<INetComponent> nets, string jtagIdentifier, string jtagBlacklist)
        {
            jtagNets = TestCoverageObjectDeterminer.GetJtagNets(nets, jtagIdentifier, jtagBlacklist);
            return jtagNets;
        }

        public IList<INetComponent> DefinePowerNets(IList<INetComponent> nets, string powerIdentifier, string powerBlacklist)
        {
            powerNets = TestCoverageObjectDeterminer.GetPowerNets(nets, powerIdentifier, powerBlacklist);
            return powerNets;
        }

        public IList<IPCBComponent> DefinePullDownResistors(IList<INetComponent> net = null)
        {
            if (net == null)
            {
                net = this.gndNets;
            }

            pullDowns = TestCoverageObjectDeterminer.GetPullUpDownResistors(net);
            return pullDowns;
        }

        public IList<IPCBComponent> DefinePullUpResistors(IList<INetComponent> powerNets = null)
        {
            if (powerNets == null)
            {
                powerNets = this.powerNets;
            }

            pullUps = TestCoverageObjectDeterminer.GetPullUpDownResistors(powerNets);
            return pullUps;
        }

        public async Task<float> GetTestCoveragePercentageValue(IList<IPCBComponent> pullUpsDowns = null, IList<IPCBComponent> ics = null)
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

            return (float)pullUpsDowns.Count / baseValue * 100;
        }

        public async Task<float> GetTestCoveragePercentageValueForAllOtherObjects(IList<IPCBComponent> ics = null, IList<IPCBComponent> pullUpsDowns = null)
        {
            if (ics == null)
            {
                ics = this.ics;
            }

            IList<IPCBComponent> resistors = pullUpsDowns;
            if (resistors == null)
            {
                resistors = new List<IPCBComponent>(pullDowns);
                foreach (var comp in pullUps)
                {
                    resistors.Add(comp);
                }
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

            IList<IPCBComponent> compsToUse = new List<IPCBComponent>();
            int foundObjects = 0;
            await Task.Run(() =>
            {
                foreach (var comp in ics)
                {
                    foreach (var pin in comp.Connections)
                    {
                        foreach (var net in pin.Nets)
                        {
                            foreach (var compToUse in net.Components)
                            {
                                if (!compsToUse.Contains(compToUse) && !IsComponentInList(compToUse, ics) && !IsComponentInList(compToUse, resistors))
                                {
                                    compsToUse.Add(compToUse);
                                }
                            }
                        }
                    }
                }

                foundObjects = GetAmountOfNetObjectsFromList(compsToUse, ics);
            }).ConfigureAwait(false);

            return (float)foundObjects / baseNumber * 100;
        }

        public async Task<IList<ITestCoverageResult>> DefineTestCoverageForPullUpDownObjects(IList<IPCBComponent> ics = null, IList<IPCBComponent> pullUpsDowns = null, bool usePullDown = true)
        {
            bool usePullUp = false;
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
                    usePullUp = true;
                    usePullDown = true;
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
                else if (!usePullDown)
                {
                    usePullUp = true;
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
            if (usePullDown)
            {
                jtagPullDownTestsPerformed = !jtagPullDownTestsPerformed;
            }

            if (usePullUp)
            {
                jtagPullUpTestsPerformed = !jtagPullUpTestsPerformed;
            }

            return results;
        }

        public async Task<float> GetTestCoveragePercentageValueForIcs(IList<IPCBComponent> ics = null)
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

            return (float)ics.Count / baseValue * 100;
        }

        private static bool IsComponentInList(IPCBComponent comp, IList<IPCBComponent> list)
        {
            foreach (var ic in list)
            {
                if (comp == ic)
                {
                    return true;
                }
            }

            return false;
        }

        private static int GetAmountOfNetObjectsFromList(IList<IPCBComponent> componentsToUse, IList<IPCBComponent> ics)
        {
            IList<INetComponent> nets = new List<INetComponent>();
            foreach (var comp in componentsToUse)
            {
                foreach (var pin in comp.Connections)
                {
                    foreach (var net in pin.Nets)
                    {
                        foreach (var compInner in ics)
                        {
                            foreach (var pinInner in compInner.Connections)
                            {
                                foreach (var netInner in pinInner.Nets)
                                {
                                    if (net == netInner && !nets.Contains(net))
                                    {
                                        nets.Add(net);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return nets.Count;
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
