using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.PcbInvestigator.Enums
{
    public enum PinComponentType
    {
        /// <summary>
        /// Unknown
        /// </summary>
        Unknown = 0,

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
}
