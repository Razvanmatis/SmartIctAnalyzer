using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Google.Protobuf.Collections;
using GrpcApi.Interfaces;
using Interfaces.PCBApiObjects;
using Interfaces.PcbInvestigator;
using Interfaces.PcbInvestigator.Enums;
using PCBI.Automation;
using PCBI.Automation.Interfaces;

namespace GrpcApi.Handler
{
    /// <summary>
    /// PcbInvestigatorApiConverter.
    /// </summary>
    public class PcbInvestigatorApiConverter : IPcbInvestigatorApiConverter
    {
        /// <summary>
        /// The default function for determining whether a testpoint is defined or not
        /// </summary>
        private static Func<IFunctionalAttributes, bool> defIsTestPoint = new Func<IFunctionalAttributes, bool>((x) => x.PartName.Contains("testpoint") || x.PackageName.Contains("TP") || x.PartName.Contains("TP"));

        /// <summary>
        /// The function to use for determining whether a testpoint is present
        /// </summary>
        private Func<IFunctionalAttributes, bool> isTestPoint;

        /// <summary>
        /// Initializes a new instance of the <see cref="PcbInvestigatorApiConverter"/> class.
        /// Constructor.
        /// </summary>
        /// <param name="isTestPoint">the is testpoint function</param>
        public PcbInvestigatorApiConverter(Func<IFunctionalAttributes, bool> isTestPoint = null)
        {
            this.isTestPoint = isTestPoint;
            if (this.isTestPoint == null)
            {
                this.isTestPoint = defIsTestPoint;
            }
        }

        /// <summary>
        /// Get all converted objects by PCB investigator objects
        /// </summary>
        /// <param name="pcbObjects">all objects from pcb investigator</param>
        /// <param name="allNets">all nets from pcb investigator</param>
        /// <returns>the result</returns>
        public IGrpcResult GetConvertedObjects(IList<InterfaceCMPObject> pcbObjects, IList<InterfaceNet> allNets)
        {
            Dictionary<InterfacePin, IPinComponent> allPins = new Dictionary<InterfacePin, IPinComponent>();
            Dictionary<InterfaceCMPObject, IPCBComponent> allComponents = new Dictionary<InterfaceCMPObject, IPCBComponent>();
            Dictionary<InterfaceNet, INetComponent> allNetsDict = new Dictionary<InterfaceNet, INetComponent>();
            if (isTestPoint == null)
            {
                isTestPoint = defIsTestPoint;
            }

            foreach (InterfaceCMPObject component in pcbObjects)
            {
                IGeometricAttributes geometricAttributes = GetComponentGeometricAttributes(component);
                IFunctionalAttributes functionalAttributes = GetFunctionalAttributes(component);
                IList<IPinComponent> connections = GetPinConnections(component, allPins);
                IPCBComponent pcbComponent = new PCBComponent(geometricAttributes, functionalAttributes, connections);
                allComponents.Add(component, pcbComponent);
            }

            UpdateNets(allPins, allComponents, allNetsDict, allNets);

            return new GrpcResult(allPins.Values.ToList(), allNetsDict.Values.ToList(), allComponents.Values.ToList());
        }

        public IList<NetGrpc> GetNetsGrpc(IList<IPinComponent> allPins, IList<IPCBComponent> allComponents, IList<INetComponent> allNets)
        {
            List<NetGrpc> nets = new List<NetGrpc>();
            foreach (INetComponent net in allNets)
            {
                NetGrpc netGrpc = new NetGrpc()
                {
                    NetName = net.NetName,
                };
                netGrpc.Components.AddRange(GetComponentsOfNetGrpc(net.Components, allComponents));
                netGrpc.Pins.AddRange(GetAllPinsOfNet(net.Pins, allPins));
                nets.Add(netGrpc);
            }

            return nets;
        }

        public IList<ComponentGrpc> GetComponentsGrpc(IList<IPCBComponent> allComponents, IList<IPinComponent> allPins)
        {
            List<ComponentGrpc> components = new List<ComponentGrpc>();
            foreach (IPCBComponent comp in allComponents)
            {
                ComponentGrpc compGrpc = new ComponentGrpc()
                {
                    Functionals = GetFunctionalAttributesGrpc(comp.FunctionalAttributes),
                    Geometrics = GetGeometricsGrpc(comp.GeometricAttributes),
                };
                compGrpc.Pins.AddRange(GetAllPinsOfComponentGrpc(comp.Connections, allPins));
                components.Add(compGrpc);
            }

            return components;
        }

        public IList<PinGrpc> GetPinsGrpc(IList<IPinComponent> allPins, IList<INetComponent> allNets)
        {
            List<PinGrpc> pins = new List<PinGrpc>();
            foreach (IPinComponent pin in allPins)
            {
                IList<int> nets = GetNetIdsGrpc(pin.Nets, allNets);
                PinGrpc pinGrpc = new PinGrpc()
                {
                    Geometrics = GetGeometricsGrpc(pin.GeometricAttributes),
                    PinType = GetPinTypeGrpc(pin.PinType),
                    PinNumber = pin.PinNumber,
                };
                pinGrpc.Nets.AddRange(nets);

                pins.Add(pinGrpc);
            }

            return pins;
        }

        private static IList<int> GetAllPinsOfNet(IList<IPinComponent> pins, IList<IPinComponent> allPins)
        {
            List<int> pinsGrpc = new List<int>();
            foreach (IPinComponent pin in pins)
            {
                pinsGrpc.Add(allPins.ToList().FindIndex(a => a == pin));
            }

            return pinsGrpc;
        }

        private static IList<int> GetComponentsOfNetGrpc(IList<IPCBComponent> components, IList<IPCBComponent> allComponents)
        {
            List<int> comps = new List<int>();
            foreach (IPCBComponent comp in components)
            {
                comps.Add(allComponents.ToList().FindIndex(a => a == comp));
            }

            return comps;
        }

        private static IList<int> GetAllPinsOfComponentGrpc(IList<IPinComponent> connections, IList<IPinComponent> allPins)
        {
            List<int> pins = new List<int>();
            foreach (IPinComponent pin in connections)
            {
                pins.Add(allPins.ToList().FindIndex(a => a == pin));
            }

            return pins;
        }

        private static ComponentTypeGrpc GetComponentTypeGrpc(PCBObjectType componentType)
        {
            return (ComponentTypeGrpc)((int)componentType);
        }

        private static IList<int> GetNetIdsGrpc(IList<INetComponent> nets, IList<INetComponent> allNets)
        {
            List<int> list = new List<int>();
            foreach (var net in nets)
            {
                list.Add(allNets.ToList().FindIndex(a => a == net));
            }

            return list;
        }

        private static PinTypeGrpc GetPinTypeGrpc(PinComponentType pinType)
        {
            return (PinTypeGrpc)((int)pinType);
        }

        private static FunctionalAttributesGrpc GetFunctionalAttributesGrpc(IFunctionalAttributes functionalAttributes)
        {
            return new FunctionalAttributesGrpc()
            {
                ComponentType = GetComponentTypeGrpc(functionalAttributes.ComponentType),
                LayerName = functionalAttributes.LayerName,
                PackageName = functionalAttributes.PackageName,
                PartName = functionalAttributes.PartName,
                Ref = functionalAttributes.Ref,
            };
        }

        private static PointGrpc GetPointGrpc(PointF centerPoint)
        {
            return new PointGrpc()
            {
                X = centerPoint.X,
                Y = centerPoint.Y,
            };
        }

        private static RectangleGrpc GetRectangleGrpc(Rectangle bounds)
        {
            return new RectangleGrpc()
            {
                X = bounds.X,
                Y = bounds.Y,
                Height = bounds.Height,
                Width = bounds.Width,
            };
        }

        private static GeometricsGrpc GetGeometricsGrpc(IGeometricAttributes geometricAttributes)
        {
            return new GeometricsGrpc()
            {
                Bounds = GetRectangleGrpc(geometricAttributes.Bounds),
                CenterPoint = GetPointGrpc(geometricAttributes.CenterPoint),
                Rotation = geometricAttributes.Rotation,
            };
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
        /// Get the geometric attributes for a Pin in dependency on the component
        /// </summary>
        /// <param name="pinToCheck">the pin to use</param>
        /// <param name="component">the componen to consider</param>
        /// <returns>the geometric attributes</returns>
        private static IGeometricAttributes GetPinGeometricAttributes(InterfacePin pinToCheck, InterfaceCMPObject component)
        {
            RectangleF rectangleF = pinToCheck.GetBoundsD(component).ToRectangleF();
            PointF centerPoint = pinToCheck.GetIPinPositionGeometryD().ToPointF();
            Rectangle bounds = new Rectangle(
                (int)rectangleF.X,
                (int)rectangleF.Y,
                (int)rectangleF.Width,
                (int)rectangleF.Height);
            float rotation = 0f;
            return new GeometricAttributes(bounds, centerPoint, rotation);
        }

        /// <summary>
        /// Get the pin type.
        /// </summary>
        /// <param name="type">the pint type by PCB investigator</param>
        /// <returns>my own pin type</returns>
        private static PinComponentType GetPinType(IPin.PinType type)
        {
            return (PinComponentType)((int)type);
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

        /// <summary>
        /// Get the components geometric attributes
        /// </summary>
        /// <param name="component">the component to create for</param>
        /// <returns>the geometric attributes</returns>
        private static IGeometricAttributes GetComponentGeometricAttributes(InterfaceCMPObject component)
        {
            Rectangle bounds = new Rectangle(
                (int)component.Bounds.X,
                (int)component.Bounds.Y,
                (int)component.Bounds.Width,
                (int)component.Bounds.Height);
            PointF centerPoint = component.Position;
            float rotation = component.Rotation;
            return new GeometricAttributes(bounds, centerPoint, rotation);
        }

        /// <summary>
        /// Get the functional attributes for a component
        /// </summary>
        /// <param name="component">the componente to create it for</param>
        /// <returns>the functional attributes</returns>
        private IFunctionalAttributes GetFunctionalAttributes(InterfaceCMPObject component)
        {
            PCBObjectType componentType = default;
            string reference = component.Ref;
            string partName = component.PartName;
            string layerName = component.LayerName;
            string packageName = component.UsedPackageName;
            string normalizedName = string.Empty;
            return new FunctionalAttributes(componentType, reference, partName, layerName, packageName, normalizedName, isTestPoint);
        }
    }
}
