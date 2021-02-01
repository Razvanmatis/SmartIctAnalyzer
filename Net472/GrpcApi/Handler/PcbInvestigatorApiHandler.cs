using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GrpcApi.Interfaces;
using Interfaces.PcbInvestigator;
using Interfaces.UnitTests;
using PCBI.Automation;
using PCBI.Automation.Interfaces;

namespace GrpcApi.Handler
{
    public class PcbInvestigatorApiHandler : IPcbInvestigatorApiHandler
    {
        private string pathToOdbProject;
        private IPcbInvestigatorApiConverter converter;

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
                    allNets.AddRange(step.GetNets());
                    foreach (var layer in step.GetAllLayerNames())
                    {
                        ILayer layerToLookIn = step.GetLayer(layer);
                        foreach (var comp in layerToLookIn.GetAllLayerObjects())
                        {
                            if (comp is ICMPObject compIcmp)
                            {
                                listOfObjects.Add(compIcmp);
                            }
                        }
                    }
                }

                listResults = GetTransformedResults(allNets, listOfObjects);
            }

            return listResults;
        }

        /// <summary>
        /// Get all components of the PCB investigator API
        /// </summary>
        /// <returns>a list with the new converted components</returns>
        public IGrpcResult GetAllComponents()
        {
            IAutomation.IAutomationInit();
            using (var pcbWin = IAutomation.CreateNewPCBIWindow(false))
            {
                pcbWin.LoadData(pathToOdbProject);
                IList<string> stepsNames = new List<string>();
                var listofSteps = pcbWin.GetStepList();
                IList<InterfaceCMPObject> listOfObjects = new List<InterfaceCMPObject>();
                List<InterfaceNet> allNets = new List<InterfaceNet>();
                foreach (IStep step in listofSteps)
                {
                    foreach (var layer in step.GetAllLayerNames())
                    {
                        ILayer layerToLookIn = step.GetLayer(layer);
                        foreach (var comp in layerToLookIn.GetAllLayerObjects())
                        {
                            if (comp is ICMPObject compIcmp)
                            {
                                listOfObjects.Add(compIcmp);
                            }
                        }
                    }

                    allNets.AddRange(step.GetNets());
                }

                int countPin = GetCountOfDifferentPins(listOfObjects);
                Console.WriteLine("Received objects from PCB Investigator API: ICMP objects: " + listOfObjects.Count + ", nets: " + allNets.Count + " and pins: " + countPin);
                return converter.GetConvertedObjects(listOfObjects, allNets);
            }
        }

        private static List<PcbTestObject> GetTransformedResults(List<INet> allNets, List<ICMPObject> listOfObjects)
        {
            Dictionary<ICMPObject, PcbTestObject> mapWithTestValues = new Dictionary<ICMPObject, PcbTestObject>();
            Dictionary<IPin, List<INet>> mapPinNets = new Dictionary<IPin, List<INet>>();
            foreach (var net in allNets)
            {
                foreach (var comp in net.ComponentList)
                {
                    if (!mapPinNets.ContainsKey(comp.GetIPin()))
                    {
                        mapPinNets.Add(comp.GetIPin(), new List<INet>());
                    }

                    if (!mapPinNets[comp.GetIPin()].Contains(net))
                    {
                        mapPinNets[comp.GetIPin()].Add(net);
                    }
                }
            }

            foreach (var comp in listOfObjects)
            {
                List<PinTestObject> testPins = new List<PinTestObject>();
                foreach (var pin in comp.GetPinList())
                {
                    testPins.Add(new PinTestObject(mapPinNets[pin].Select(x => x.NetName).ToList()));
                }

                PcbTestObject testResult = new PcbTestObject(comp.Ref, testPins);
                mapWithTestValues.Add(comp, testResult);
            }

            return mapWithTestValues.Values.ToList();
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
