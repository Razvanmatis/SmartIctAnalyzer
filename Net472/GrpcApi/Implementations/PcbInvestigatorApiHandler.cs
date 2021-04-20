using System;
using System.Collections.Generic;
using System.Diagnostics;
using GrpcApi.Interfaces;
using Interfaces.Helper;
using PCBI.Automation;
using PCBI.Automation.Interfaces;

namespace GrpcApi.Implementations
{
    public class PcbInvestigatorApiHandler : IPcbInvestigatorApiHandler
    {
        private static readonly AbstractDataProvider DataProvider = new NewDataProvider();
        private readonly string pathToOdbProject;
        private readonly IPcbInvestigatorApiConverter converter;

        public PcbInvestigatorApiHandler(string pathToOdbProject, IPcbInvestigatorApiConverter converter)
        {
            this.converter = converter;
            this.pathToOdbProject = pathToOdbProject;
        }

        public static List<PcbTestObject> GetAllObjectsFromPcbInvestigator(string pathToUse)
        {
            List<INet> allNets = new List<INet>();
            List<ICMPObject> listOfObjects = new List<ICMPObject>();
            List<PcbTestObject> listResults;
            IAutomation.IAutomationInit();
            using (var pcbWin = IAutomation.CreateNewPCBIWindow(false))
            {
                pcbWin.LoadData(pathToUse);
                var listofSteps = pcbWin.GetStepList();
                foreach (IStep step in listofSteps)
                {
                    List<INet> stepNets = step.GetNets();
                    foreach (var net in stepNets)
                    {
                        if (!allNets.Contains(net))
                        {
                            allNets.Add(net);
                        }
                    }

                    foreach (var layer in step.GetAllLayerNames())
                    {
                        ILayer layerToLookIn = step.GetLayer(layer);
                        foreach (var comp in layerToLookIn.GetAllLayerObjects())
                        {
                            if (comp is ICMPObject compIcmp && !listOfObjects.Contains(compIcmp))
                            {
                                listOfObjects.Add(compIcmp);
                            }
                        }
                    }
                }

                listResults = DataProvider.GetTransformedResults(allNets, listOfObjects);
            }

            return listResults;
        }

        public IGrpcResult GetAllComponents(string steps)
        {
            Stopwatch sw = Stopwatch.StartNew();
            IAutomation.IAutomationInit();
            using (var pcbWin = IAutomation.CreateNewPCBIWindow(false))
            {
                pcbWin.LoadData(pathToOdbProject);
                var listofSteps = pcbWin.GetStepList();
                List<int> stepToConsider = GetStepsToBeConsidered(steps, listofSteps.Count);
                IList<InterfaceCMPObject> listOfObjects = new List<InterfaceCMPObject>();
                List<InterfaceNet> allNets = new List<InterfaceNet>();
                int stepNo = 0;
                foreach (IStep step in listofSteps)
                {
                    stepNo++;
                    if (!stepToConsider.Contains(stepNo))
                    {
                        continue;
                    }

                    foreach (var layer in step.GetAllLayerNames())
                    {
                        ILayer layerToLookIn = step.GetLayer(layer);
                        foreach (var comp in layerToLookIn.GetAllLayerObjects())
                        {
                            if (comp is ICMPObject compIcmp && !listOfObjects.Contains(compIcmp))
                            {
                                listOfObjects.Add(compIcmp);
                            }
                        }
                    }

                    foreach (var net in step.GetNets())
                    {
                        if (!allNets.Contains(net))
                        {
                            allNets.Add(net);
                        }
                    }
                }

                sw.Stop();
                Console.WriteLine(LangRessource.ReadOutByPcbInvestigator, sw.Elapsed.TotalMilliseconds);
                int countPin = GetCountOfDifferentPins(listOfObjects);
                Console.WriteLine("Received objects from PCB Investigator API: ICMP objects: " + listOfObjects.Count + ", nets: " + allNets.Count + " and pins: " + countPin);
                sw = Stopwatch.StartNew();
                var result = converter.GetConvertedObjects(listOfObjects, allNets, DataProvider);
                sw.Stop();
                Console.WriteLine(LangRessource.ParseAllObjectsIntoSerializable, sw.Elapsed.TotalMilliseconds);
                return result;
            }
        }

        private static List<int> GetStepsToBeConsidered(string steps, int count)
        {
            List<int> stepsToConsider = new List<int>();
            bool useAll = false;
            if (steps.Equals("-1"))
            {
                useAll = true;
            }
            else
            {
                string[] stepsArray = steps.Split(';');
                foreach (var st in stepsArray)
                {
                    if (int.TryParse(st, out int value))
                    {
                        stepsToConsider.Add(value);
                    }
                    else
                    {
                        Console.WriteLine("Invalid try to parse a number: " + st + ", using all steps!");
                        useAll = true;
                        break;
                    }
                }
            }

            if (useAll)
            {
                for (int index = 1; index <= count; index++)
                {
                    if (!stepsToConsider.Contains(index))
                    {
                        stepsToConsider.Add(index);
                    }
                }
            }

            return stepsToConsider;
        }

        private static int GetCountOfDifferentPins(IList<InterfaceCMPObject> listOfObjects)
        {
            List<IPin> pins = new List<IPin>();
            foreach (var obj in listOfObjects)
            {
                foreach (var pin in obj.GetPinList())
                {
                    if (!pins.Contains(pin))
                    {
                        pins.Add(pin);
                    }
                }
            }

            return pins.Count;
        }
    }
}
