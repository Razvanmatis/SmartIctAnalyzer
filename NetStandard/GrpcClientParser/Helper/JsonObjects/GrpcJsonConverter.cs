using System;
using System.Collections.Generic;
using System.Text;
using Google.Protobuf.Collections;
using GrpcClientParser.Interfaces;

namespace GrpcClientParser.Helper.JsonObjects
{
    public static class GrpcJsonConverter
    {
        public static IResultJson GetResultJson(Result result)
        {
            IList<ComponentJson> comps = new List<ComponentJson>();
            IList<PinJson> pins = new List<PinJson>();
            IList<NetJson> nets = new List<NetJson>();
            foreach (var comp in result.Components.Values)
            {
                comps.Add(GetComponentJson(comp));
            }

            foreach (var pin in result.Pins.Values)
            {
                pins.Add(GetPinJson(pin));
            }

            foreach (var net in result.Nets.Values)
            {
                nets.Add(GetNetJson(net));
            }

            return new ResultJson(pins, comps, nets);
        }

        public static ComponentJson GetComponentJson(ComponentGrpc grpc)
        {
            return new ComponentJson(GetGeometricJson(grpc.Geometrics), GetFunctionalJson(grpc.Functionals), GetListOfNumbers(grpc.Pins));
        }

        public static PinJson GetPinJson(PinGrpc pin)
        {
            return new PinJson(GetListOfNumbers(pin.Nets), GetGeometricJson(pin.Geometrics), GetPinTypeJson(pin.PinType));
        }

        public static NetJson GetNetJson(NetGrpc net)
        {
            return new NetJson(GetListOfNumbers(net.Pins), GetListOfNumbers(net.Components), net.NetName);
        }

        private static PinTypeJson GetPinTypeJson(PinTypeGrpc pinType)
        {
            if (pinType == PinTypeGrpc.PinEllipse)
            {
                return PinTypeJson.PinEllipse;
            }
            else if (pinType == PinTypeGrpc.PinPolygon)
            {
                return PinTypeJson.PinPolygon;
            }
            else if (pinType == PinTypeGrpc.PinRectangle)
            {
                return PinTypeJson.PinRectangle;
            }
            else
            {
                return PinTypeJson.UnknownPinType;
            }
        }

        private static IList<int> GetListOfNumbers(RepeatedField<int> values)
        {
            IList<int> list = new List<int>();
            foreach (var value in values)
            {
                list.Add(value);
            }

            return list;
        }

        private static FunctionalAttributesJson GetFunctionalJson(FunctionalAttributesGrpc functionals)
        {
            return new FunctionalAttributesJson(GetComponentTypeJson(functionals.ComponentType), functionals.Ref, functionals.PartName, functionals.LayerName, functionals.PackageName, string.Empty, string.Empty);
        }

        private static ComponentTypeJson GetComponentTypeJson(ComponentTypeGrpc componentType)
        {
            if (componentType == ComponentTypeGrpc.Arc)
            {
                return ComponentTypeJson.Arc;
            }
            else if (componentType == ComponentTypeGrpc.Component)
            {
                return ComponentTypeJson.Component;
            }
            else if (componentType == ComponentTypeGrpc.Line)
            {
                return ComponentTypeJson.Line;
            }
            else if (componentType == ComponentTypeGrpc.Pad)
            {
                return ComponentTypeJson.Pad;
            }
            else if (componentType == ComponentTypeGrpc.Surface)
            {
                return ComponentTypeJson.Surface;
            }
            else if (componentType == ComponentTypeGrpc.Symbol)
            {
                return ComponentTypeJson.Symbol;
            }
            else if (componentType == ComponentTypeGrpc.Text)
            {
                return ComponentTypeJson.Text;
            }
            else
            {
                return ComponentTypeJson.UnknownComponentType;
            }
        }

        private static GeometricJson GetGeometricJson(GeometricsGrpc geometrics)
        {
            return new GeometricJson(GetRectangleJson(geometrics.Bounds), GetPointJson(geometrics.CenterPoint), geometrics.Rotation);
        }

        private static PointJson GetPointJson(PointGrpc centerPoint)
        {
            return new PointJson(centerPoint.X, centerPoint.Y);
        }

        private static RectangleJson GetRectangleJson(RectangleGrpc bounds)
        {
            return new RectangleJson(bounds.X, bounds.Y, bounds.Width, bounds.Height);
        }
    }
}
