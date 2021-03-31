using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GrpcApi.Interfaces;
using Interfaces.PcbInvestigator;
using Interfaces.PcbInvestigator.Enums;
using PCBI.Automation.Interfaces;

namespace GrpcApi.Implementations
{
    /// <summary>
    /// PcbInvestigatorApiConverter.
    /// </summary>
    public class PcbInvestigatorApiConverter : IPcbInvestigatorApiConverter
    {
        /// <summary>
        /// The default function for determining whether a testpoint is defined or not
        /// </summary>
        public static readonly Func<IFunctionalAttributes, bool> DefIsTestPoint = new Func<IFunctionalAttributes, bool>((x) => x.PartName.Contains("testpoint") || x.PackageName.Contains("TP") || x.PartName.Contains("TP"));

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
                this.isTestPoint = DefIsTestPoint;
            }
        }

        /// <summary>
        /// Get all converted objects by PCB investigator objects
        /// </summary>
        /// <param name="pcbObjects">all objects from pcb investigator</param>
        /// <param name="allNets">all nets from pcb investigator</param>
        /// <returns>the result</returns>
        public IGrpcResult GetConvertedObjects(IList<InterfaceCMPObject> pcbObjects, IList<InterfaceNet> allNets, AbstractDataProvider dataProvider)
        {
            return dataProvider.GetConvertedObjects(pcbObjects, allNets, ref isTestPoint);
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
    }
}
