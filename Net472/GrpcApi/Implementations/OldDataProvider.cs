using System;
using System.Collections.Generic;
using System.Linq;
using GrpcApi.Interfaces;
using Interfaces.Helper;
using Interfaces.PcbInvestigator;
using Interfaces.PcbInvestigator.Enums;
using Interfaces.PcbInvestigator.Implementations;
using PCBI.Automation;
using PCBI.Automation.Interfaces;

namespace GrpcApi.Implementations
{
    public class OldDataProvider : AbstractDataProvider
    {
        public override IGrpcResult GetConvertedObjects(IList<InterfaceCMPObject> pcbObjects, IList<InterfaceNet> allNets, ref Func<IFunctionalAttributes, bool> isTestPoint)
        {
            Dictionary<InterfacePin, IPinComponent> allPins = new Dictionary<InterfacePin, IPinComponent>();
            Dictionary<InterfaceCMPObject, IPCBComponent> allComponents = new Dictionary<InterfaceCMPObject, IPCBComponent>();
            Dictionary<InterfaceNet, INetComponent> allNetsDict = new Dictionary<InterfaceNet, INetComponent>();
            if (isTestPoint == null)
            {
                isTestPoint = PcbInvestigatorApiConverter.DefIsTestPoint;
            }

            foreach (InterfaceCMPObject component in pcbObjects)
            {
                IGeometricAttributes geometricAttributes = GetComponentGeometricAttributes(component);
                IFunctionalAttributes functionalAttributes = GetFunctionalAttributes(component, isTestPoint);
                IList<IPinComponent> connections = GetPinConnections(component, allPins);
                IPCBComponent pcbComponent = new PCBComponent(geometricAttributes, functionalAttributes, connections);
                allComponents.Add(component, pcbComponent);
            }

            UpdateNets(allPins, allComponents, allNetsDict, allNets);

            return new GrpcResult(allPins.Values.ToList(), allNetsDict.Values.ToList(), allComponents.Values.ToList());
        }

        public override List<PcbTestObject> GetTransformedResults(List<INet> allNets, List<ICMPObject> listOfObjects)
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
                    testPins.Add(new PinTestObject(pin.PinNumber, mapPinNets[pin].Select(x => x.NetName).ToList()));
                }

                PcbTestObject testResult = new PcbTestObject(comp.Ref, testPins);
                mapWithTestValues.Add(comp, testResult);
            }

            return mapWithTestValues.Values.ToList();
        }

        /// <summary>
        /// Update all nets
        /// </summary>
        /// <param name="allPins">all pins</param>
        /// <param name="allComponents">all components</param>
        /// <param name="allNets">all created nets</param>
        /// <param name="allNetsFromSource">all nets from source</param>
        private static void UpdateNets(
            Dictionary<InterfacePin, IPinComponent> allPins,
            Dictionary<InterfaceCMPObject, IPCBComponent> allComponents,
            Dictionary<InterfaceNet, INetComponent> allNets,
            IList<InterfaceNet> allNetsFromSource)
        {
            Dictionary<IPinComponent, IList<INetComponent>> pinNetConnections = new Dictionary<IPinComponent, IList<INetComponent>>();
            foreach (var pin in allPins.Values)
            {
                pin.Nets.Clear();
            }

            foreach (InterfaceNet netObjectFromSource in allNetsFromSource)
            {
                if (!allNets.ContainsKey(netObjectFromSource))
                {
                    IList<IPinComponent> listOfPins = GetPinConnectionsOfNet(netObjectFromSource.ComponentList, allPins);
                    INetComponent net = new NetComponent(netObjectFromSource.NetName, listOfPins, GetComponentsOfNet(netObjectFromSource.ComponentList, allComponents));
                    allNets.Add(netObjectFromSource, net);
                    foreach (var pin in listOfPins)
                    {
                        if (!pinNetConnections.ContainsKey(pin))
                        {
                            pinNetConnections.Add(pin, new List<INetComponent>());
                        }

                        if (!pinNetConnections[pin].Contains(net))
                        {
                            pinNetConnections[pin].Add(net);
                        }
                    }
                }
            }

            foreach (InterfaceNet netObjectFromSource in allNetsFromSource)
            {
                foreach (var comp in netObjectFromSource.ComponentList)
                {
                    if (pinNetConnections.ContainsKey(allPins[comp.GetIPin()]))
                    {
                        foreach (var net in pinNetConnections[allPins[comp.GetIPin()]])
                        {
                            if (!allPins[comp.GetIPin()].Nets.Contains(net))
                            {
                                allPins[comp.GetIPin()].Nets.Add(net);
                            }
                        }
                    }
                }
            }
        }

        private static IList<IPCBComponent> GetComponentsOfNet(List<INetObject> componentList, Dictionary<InterfaceCMPObject, IPCBComponent> allComponents)
        {
            IList<IPCBComponent> list = new List<IPCBComponent>();
            foreach (var net in componentList)
            {
                list.Add(allComponents[net.ICMP]);
            }

            return list;
        }

        private static IList<IPinComponent> GetPinConnectionsOfNet(List<INetObject> componentList, Dictionary<InterfacePin, IPinComponent> allPins)
        {
            IList<IPinComponent> listPins = new List<IPinComponent>();
            foreach (var net in componentList)
            {
                listPins.Add(allPins[net.GetIPin()]);
            }

            return listPins;
        }

        /// <summary>
        /// Gets all pin connections of one component
        /// </summary>
        /// <param name="component">the component to look at</param>
        /// <param name="allPins">all pins dict</param>
        /// <returns>a list containing all pins of that component</returns>
        private static IList<IPinComponent> GetPinConnections(InterfaceCMPObject component, Dictionary<InterfacePin, IPinComponent> allPins)
        {
            IList<IPinComponent> list = new List<IPinComponent>();
            foreach (InterfacePin pinToCheck in component.GetPinList())
            {
                if (!allPins.ContainsKey(pinToCheck))
                {
                    IGeometricAttributes geometricAttributes = GetPinGeometricAttributes(pinToCheck, component);
                    PinComponentType pinType = GetPinType(pinToCheck.Type);
                    IPinComponent pin = new PinComponent(geometricAttributes, pinType, new List<INetComponent>(), pinToCheck.PinNumber);
                    allPins.Add(pinToCheck, pin);
                }

                list.Add(allPins[pinToCheck]);
            }

            return list;
        }
    }
}
