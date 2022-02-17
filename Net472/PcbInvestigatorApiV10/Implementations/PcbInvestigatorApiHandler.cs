using System;
using System.Collections.Generic;
using System.Diagnostics;
using GrpcApi.Helper;
using GrpcApi.Implementations;
using GrpcApi.Interfaces;
using PCBI.Automation;
using PCBI.Automation.Interfaces;
using PcbInvestigatorApiV10.Interfaces;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Interfaces.PcbInvestigator;

namespace PcbInvestigatorApiV10.Implementations
{
    public class PcbInvestigatorApiHandler : IPcbInvestigatorApiHandler
    {
        private AbstractDataProvider DataProvider = new NewDataProvider();

        public string PathToOdb { get; set; }

        public IPcbInvestigatorApiBaseConverter Converter { get; set; }

        public List<PcbTestObject> GetAllObjectsFromPcbInvestigator(string pathToUse)
        {
            List<INet> allNets = new List<INet>();
            List<ICMPObject> listOfObjects = new List<ICMPObject>();
            List<PcbTestObject> listResults;
            IAutomation.IAutomationInit();
            using (var pcbWin = IAutomation.CreateNewPCBIWindow(false))
            {
                pcbWin.LoadData(pathToUse, out _);
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
            try
            {
                using (var pcbWin = IAutomation.CreateNewPCBIWindow(false, true))
                {
                    pcbWin.LoadData(PathToOdb, out _);
                    var listofSteps = pcbWin.GetStepList();
                    List<int> stepToConsider = GetStepsToBeConsidered(steps, listofSteps.Count);
                    IList<InterfaceCMPObject> listOfObjects = new List<InterfaceCMPObject>();
                    IList<StepComponentContainer> listOfStepObjects = new List<StepComponentContainer>();
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
                                    listOfStepObjects.Add(new StepComponentContainer(compIcmp, stepNo));
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
                    Console.WriteLine(
                        "Received objects from PCB Investigator API: ICMP objects: "
                        + listOfObjects.Count
                        + ", nets: " + allNets.Count
                        + " and pins: "
                        + countPin);
                    sw = Stopwatch.StartNew();
                    var result = ((IPcbInvestigatorApiConverter)Converter).GetConvertedObjects(
                        listOfStepObjects, allNets, DataProvider, listofSteps.Count);
                    sw.Stop();
                    Console.WriteLine(LangRessource.ParseAllObjectsIntoSerializable, sw.Elapsed.TotalMilliseconds);
                    return result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return new GrpcResult(new List<IPinComponent>(), new List<INetComponent>(), new List<IPCBComponent>(), 0);
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
            HashSet<IPin> pins = new HashSet<IPin>();
            foreach (var obj in listOfObjects)
            {
                foreach (var pin in obj.GetPinList())
                {
                    pins.Add(pin);
                }
            }

            return pins.Count;
        }
    }
}
