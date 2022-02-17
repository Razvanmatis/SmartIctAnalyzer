using System.Collections.Generic;

namespace ProMik.SmartIct.PCBComponentParser.Helper.JsonObjects
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
        public PinJson(IList<int> nets, GeometricJson geometrics, PinTypeJson pinType, string pinNumber)
        {
            Nets = nets;
            Geometrics = geometrics;
            PinType = pinType;
            PinNumber = pinNumber;
        }

        public IList<int> Nets { get; set; }

        public GeometricJson Geometrics { get; set; }

        public PinTypeJson PinType { get; set; }

        public string PinNumber { get; set; }
    }
}
