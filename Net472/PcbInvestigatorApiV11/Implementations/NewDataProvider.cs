using System;
using System.Collections.Generic;
using System.Linq;
using GrpcApi.Helper;
using GrpcApi.Implementations;
using GrpcApi.Interfaces;
using PCBI.Automation;
using PCBI.Automation.Interfaces;
using PcbInvestigatorApiV11.Interfaces;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Implementations;

namespace PcbInvestigatorApiV11.Implementations
{
    public class NewDataProvider : AbstractDataProvider
    {
        public override IGrpcResult GetConvertedObjects(
            IList<StepComponentContainer> pcbObjects,
            IList<InterfaceNet> allNets,
            ref Func<IFunctionalAttributes, bool> isTestPoint,
            int stepAmount)
        {
            Dictionary<InterfaceNet, List<InterfaceCMPObject>> netToComponents
                = new Dictionary<InterfaceNet, List<InterfaceCMPObject>>();
            Dictionary<InterfaceCMPObject, IPCBComponent> allComponents = new Dictionary<InterfaceCMPObject, IPCBComponent>();
            Dictionary<InterfaceNet, INetComponent> allNetsDist = new Dictionary<InterfaceNet, INetComponent>();
            List<IPinComponent> allPinsDist = new List<IPinComponent>();
            if (isTestPoint == null)
            {
                isTestPoint = PcbInvestigatorApiConverter.DefIsTestPoint;
            }

            foreach (var net in allNets)
            {
                allNetsDist.Add(net, new NetComponent(net.NetName, new List<IPinComponent>(), new List<IPCBComponent>()));
            }

            foreach (var component in pcbObjects)
            {
                if (component.Component is ICMPObject icmp)
                {
                    IGeometricAttributes geometricAttributes = GetComponentGeometricAttributes(icmp);
                    IFunctionalAttributes functionalAttributes = GetFunctionalAttributes(icmp, isTestPoint, component.StepNo);
                    IList<IPinComponent> connections = new List<IPinComponent>();
                    foreach (var net in allNets)
                    {
                        foreach (var comp in net.ComponentList)
                        {
                            if (comp.ICMP == icmp)
                            {
                                IPinComponent pin = new PinComponent(
                                    GetPinGeometricAttributes(comp.GetIPin(), comp.ICMP),
                                    GetPinType(comp.GetIPin().Type),
                                    new List<INetComponent>() { allNetsDist[net] },
                                    comp.GetIPin().PinNumber);
                                connections.Add(pin);
                                allPinsDist.Add(pin);
                                if (!netToComponents.ContainsKey(net))
                                {
                                    netToComponents.Add(net, new List<InterfaceCMPObject>() { comp.ICMP });
                                }
                                else
                                {
                                    netToComponents[net].Add(comp.ICMP);
                                }

                                if (!allNetsDist[net].Pins.Contains(pin))
                                {
                                    allNetsDist[net].Pins.Add(pin);
                                }
                            }
                        }
                    }

                    IPCBComponent pcbComponent = new PCBComponent(geometricAttributes, functionalAttributes, connections);
                    allComponents.Add(component.Component, pcbComponent);
                }
            }

            foreach (var net in netToComponents)
            {
                foreach (var comp in net.Value)
                {
                    if (!allNetsDist[net.Key].Components.Contains(allComponents[comp]))
                    {
                        allNetsDist[net.Key].Components.Add(allComponents[comp]);
                    }
                }
            }

            return new GrpcResult(allPinsDist, allNetsDist.Values.ToList(), allComponents.Values.ToList(), stepAmount);
        }

        public override List<PcbTestObject> GetTransformedResults(List<INet> allNets, List<ICMPObject> listOfObjects)
        {
            List<PcbTestObject> results = new List<PcbTestObject>();
            foreach (var icmp in listOfObjects)
            {
                List<INet> nets = new List<INet>();
                List<PinTestObject> newPins = new List<PinTestObject>();
                foreach (var net in allNets)
                {
                    foreach (var comp in net.ComponentList)
                    {
                        if (comp.ICMP == icmp && !nets.Contains(net))
                        {
                            nets.Add(net);
                            newPins.Add(new PinTestObject(comp.GetIPin().PinNumber, new List<string>() { net.NetName }));
                        }
                    }
                }

                results.Add(new PcbTestObject(icmp.Ref, newPins));
            }

            return results;
        }
    }
}
