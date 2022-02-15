using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Newtonsoft.Json;
using ProMik.SmartIct.Interfaces.GrpcClientParser;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Enums;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Implementations;
using ProMik.SmartIct.PCBComponentParser.Implementations;
using ProMik.SmartIct.PCBComponentParser.Interfaces;

namespace ProMik.SmartIct.PCBComponentParser.Helper.JsonObjects
{
    public static class JsonImportExportHelper
    {
        public static string GetDataAsString(IParsedResult result, ILogger logger)
        {
            IList<IPinComponent> pins = new List<IPinComponent>();
            foreach (var comp in result.Components)
            {
                foreach (var pin in comp.Connections)
                {
                    if (!pins.Contains(pin))
                    {
                        pins.Add(pin);
                    }
                }
            }

            IList<NetJson> netJsons = new List<NetJson>();
            IList<ComponentJson> compJson = new List<ComponentJson>();
            IList<PinJson> pinJson = new List<PinJson>();

            foreach (var comp in result.Components)
            {
                compJson.Add(new ComponentJson(GetGeometricsJson(comp.GeometricAttributes), GetFunctionalJson(comp.FunctionalAttributes), GetPinJson(comp.Connections, pins)));
            }

            foreach (var net in result.Nets)
            {
                netJsons.Add(new NetJson(GetPinJson(net.Pins, pins), GetComponentsJson(net.Components, result.Components), net.NetName));
            }

            foreach (var pin in pins)
            {
                pinJson.Add(new PinJson(GetNetsJson(pin.Nets, result.Nets), GetGeometricsJson(pin.GeometricAttributes), GetPinTypeJson(pin.PinType), pin.PinNumber));
            }

            IResultJson resultJson = new ResultJson(pinJson, compJson, netJsons, result.AmountSteps);
            return GetSerializedData(resultJson, logger);
        }

        public static IList<IPCBComponent> GetParsedComponents(
            IResultJson result,
            IList<string> rIdentifier,
            IList<string> cIdentifier,
            IList<string> iIdentifier,
            IList<string> icIdentifier,
            IList<string> conIdentifier,
            Func<IFunctionalAttributes, bool> isTestpoint,
            bool useContains = false)
        {
            IList<IPinComponent> pins = GetParsedPins(result.Pins);
            IList<IPCBComponent> components = GetParsedComponents(
                result.Components,
                pins,
                rIdentifier,
                cIdentifier,
                iIdentifier,
                icIdentifier,
                conIdentifier,
                isTestpoint,
                useContains);
            IList<INetComponent> nets = GetParsedNets(result.Nets, components, pins);
            UpdateAllParsedPins(pins, nets, result.Pins);
            return components;
        }

        public static IList<INetComponent> GetParsedNets(IList<IPCBComponent> components)
        {
            List<INetComponent> nets = new List<INetComponent>();
            foreach (IPCBComponent comp in components)
            {
                foreach (IPinComponent pin in comp.Connections)
                {
                    foreach (var netToCheck in pin.Nets)
                    {
                        if (!nets.Contains(netToCheck))
                        {
                            nets.Add(netToCheck);
                        }
                    }
                }
            }

            return nets;
        }

        public static IParsedResult ImportComponentsFromFile(
           string data,
           IList<string> rIdentifier,
           IList<string> cIdentifier,
           IList<string> iIdentifier,
           IList<string> icIdentifier,
           IList<string> conIdentifier,
           Func<IFunctionalAttributes, bool> isTestPoint,
           ILogger logger,
           bool useContains = false)
        {
            IResultJson resultJson = GetResultJsonFromData(data, logger);
            if (resultJson != null)
            {
                IList<IPCBComponent> components = GetParsedComponents(
                resultJson,
                rIdentifier,
                cIdentifier,
                iIdentifier,
                icIdentifier,
                conIdentifier,
                isTestPoint,
                useContains);
                IList<INetComponent> nets = GetParsedNets(components);
                return new ParsedResult(components, nets, resultJson.StepAmount);
            }
            else
            {
                return null;
            }
        }

        private static IResultJson GetResultJsonFromData(string content, ILogger logger)
        {
            if (string.IsNullOrEmpty(content))
            {
                logger.LogMessage("Read out text was empty!", LogCategory.ERROR);
                return null;
            }

            ResultJson result = null;
            try
            {
                result = JsonConvert.DeserializeObject<ResultJson>(content);
            }
            catch (JsonException e)
            {
                logger.LogMessage("Error at parsing file for getting all objects: " + e.Message, LogCategory.ERROR);
            }

            return result;
        }

        private static string GetSerializedData(IResultJson resultJson, ILogger logger)
        {
            try
            {
                return JsonConvert.SerializeObject(resultJson, Formatting.Indented);
            }
            catch (JsonException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return string.Empty;
            }
        }

        private static PinTypeJson GetPinTypeJson(PinComponentType pinType)
        {
            return (PinTypeJson)(int)pinType;
        }

        private static IList<int> GetNetsJson(IList<INetComponent> netsToFind, IList<INetComponent> allNets)
        {
            IList<int> list = new List<int>();
            foreach (var net in netsToFind)
            {
                list.Add(allNets.IndexOf(net));
            }

            return list;
        }

        private static IList<int> GetComponentsJson(IList<IPCBComponent> componentsToFind, IList<IPCBComponent> allComponents)
        {
            IList<int> list = new List<int>();
            foreach (var comp in componentsToFind)
            {
                list.Add(allComponents.IndexOf(comp));
            }

            return list;
        }

        private static IList<int> GetPinJson(IList<IPinComponent> connections, IList<IPinComponent> pins)
        {
            IList<int> list = new List<int>();
            foreach (var pin in connections)
            {
                list.Add(pins.IndexOf(pin));
            }

            return list;
        }

        private static FunctionalAttributesJson GetFunctionalJson(IFunctionalAttributes functionalAttributes)
        {
            return new FunctionalAttributesJson(
                GetComponentTypeJson(
                functionalAttributes.ComponentType),
                functionalAttributes.Ref,
                functionalAttributes.PartName,
                functionalAttributes.LayerName,
                functionalAttributes.PackageName,
                functionalAttributes.Value,
                functionalAttributes.NormalizedName,
                functionalAttributes.StepNo);
        }

        private static ComponentTypeJson GetComponentTypeJson(PCBObjectType componentType)
        {
            return (ComponentTypeJson)(int)componentType;
        }

        private static GeometricJson GetGeometricsJson(IGeometricAttributes geometricAttributes)
        {
            return new GeometricJson(GetRectangleJson(geometricAttributes.Bounds), GetPointJson(geometricAttributes.CenterPoint), geometricAttributes.Rotation, geometricAttributes.CompHeight);
        }

        private static PointJson GetPointJson(PointF centerPoint)
        {
            return new PointJson(centerPoint.X, centerPoint.Y);
        }

        private static RectangleJson GetRectangleJson(Rectangle bounds)
        {
            return new RectangleJson(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }

        private static void UpdateParsedPin(IPinComponent pinComponent, IList<INetComponent> nets, int netId)
        {
            pinComponent.Nets.Add(nets.ToList()[netId]);
        }

        private static void UpdateAllParsedPins(IList<IPinComponent> parsedPins, IList<INetComponent> nets, IList<PinJson> grpcPins)
        {
            for (int idx = 0; idx < grpcPins.Count; idx++)
            {
                foreach (var netId in grpcPins[idx].Nets)
                {
                    UpdateParsedPin(parsedPins.ToList()[idx], nets, netId);
                }
            }
        }

        private static IList<IPCBComponent> GetParsedComponentList(IList<int> componentsToUse, IList<IPCBComponent> components)
        {
            List<IPCBComponent> parsedComponents = new List<IPCBComponent>();
            foreach (int idx in componentsToUse)
            {
                parsedComponents.Add(components.ToList()[idx]);
            }

            return parsedComponents;
        }

        private static IList<IPinComponent> GetParsedPinConnections(IList<int> pinConnections, IList<IPinComponent> pinComponents)
        {
            List<IPinComponent> parsedPins = new List<IPinComponent>();
            foreach (int idx in pinConnections)
            {
                parsedPins.Add(pinComponents.ToList()[idx]);
            }

            return parsedPins;
        }

        private static IList<INetComponent> GetParsedNets(IList<NetJson> nets, IList<IPCBComponent> components, IList<IPinComponent> pins)
        {
            List<INetComponent> parsedNets = new List<INetComponent>();
            foreach (var net in nets)
            {
                parsedNets.Add(new NetComponent(net.NetName, GetParsedPinConnections(net.Pins, pins), GetParsedComponentList(net.Components, components)));
            }

            return parsedNets;
        }

        private static PCBObjectType GetParsedComponentType(ComponentTypeJson componentType)
        {
            return (PCBObjectType)(int)componentType;
        }

        private static PinComponentType GetParsedPinType(PinTypeJson pinType)
        {
            return (PinComponentType)(int)pinType;
        }

        private static IGeometricAttributes GetParsedGeometricAttributes(GeometricJson geometrics)
        {
            return new GeometricAttributes(
                new Rectangle(geometrics.Bounds.X, geometrics.Bounds.Y, geometrics.Bounds.Width, geometrics.Bounds.Height),
                new PointF(geometrics.CenterPoint.X, geometrics.CenterPoint.Y),
                geometrics.Rotation,
                geometrics.CompHeight);
        }

        private static IList<IPinComponent> GetParsedPins(IList<PinJson> pins)
        {
            List<IPinComponent> parsedPins = new List<IPinComponent>();
            foreach (var pin in pins)
            {
                parsedPins.Add(new PinComponent(GetParsedGeometricAttributes(pin.Geometrics), GetParsedPinType(pin.PinType), new List<INetComponent>(), pin.PinNumber));
            }

            return parsedPins;
        }

        private static IList<IPCBComponent> GetParsedComponents(
        IList<ComponentJson> components,
        IList<IPinComponent> pins,
        IList<string> rIdentifier,
        IList<string> cIdentifier,
        IList<string> iIdentifier,
        IList<string> icIdentifier,
        IList<string> conIdentifier,
        Func<IFunctionalAttributes, bool> isTestPoint,
        bool useContains = false)
        {
            List<IPCBComponent> parsedComponents = new List<IPCBComponent>();
            foreach (var comp in components)
            {
                parsedComponents.Add(PCBComponentFactory.GetComponentByType(
                    GetParsedGeometricAttributes(comp.Geometrics),
                    GetParsedFunctionalAttributes(comp.Functionals, isTestPoint),
                    GetParsedPinConnections(comp.Pins, pins),
                    rIdentifier,
                    cIdentifier,
                    iIdentifier,
                    icIdentifier,
                    conIdentifier,
                    useContains));
            }

            return parsedComponents;
        }

        private static IFunctionalAttributes GetParsedFunctionalAttributes(FunctionalAttributesJson functionals, Func<IFunctionalAttributes, bool> isTestPoint)
        {
            return new FunctionalAttributes(
                GetParsedComponentType(functionals.ComponentType),
                functionals.Ref,
                functionals.PartName,
                functionals.LayerName,
                functionals.PackageName,
                string.Empty,
                isTestPoint,
                functionals.StepNo,
                functionals.Value);
        }
    }
}
