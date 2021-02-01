using System;
using System.Collections.Generic;
using System.Text;
using GrpcClientParser.Interfaces;

namespace GrpcClientParser.Helper.JsonObjects
{
    public enum PinTypeJson
    {
        /// <summary>
        /// UnknownPinType
        /// </summary>
        UnknownPinType = 0,

        /// <summary>
        /// PinEllipse
        /// </summary>
        PinEllipse = 1,

        /// <summary>
        /// PinRectangle
        /// </summary>
        PinRectangle = 2,

        /// <summary>
        /// PinPolygon
        /// </summary>
        PinPolygon = 3,
    }

    public class PinJson
    {
        public PinJson(IList<int> nets, GeometricJson geometrics, PinTypeJson pinType)
        {
            Nets = nets;
            Geometrics = geometrics;
            PinType = pinType;
        }

        public IList<int> Nets { get; set; }

        public GeometricJson Geometrics { get; set; }

        public PinTypeJson PinType { get; set; }
    }
}
