using ProMik.SmartIct.Interfaces.PcbInvestigator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProMik.SmartIct.Interfaces.Helper
{
    public enum PinConnectionType
    {
        /// <summary>
        /// UNKNOWN
        /// </summary>
        OTHER = 0,

        /// <summary>
        /// PULLUP
        /// </summary>
        PULLUP = 1,

        /// <summary>
        /// PULLDOWN
        /// </summary>
        PULLDOWN = 2,

        /// <summary>
        /// NOTCONNECTED
        /// </summary>
        NOTCONNECTED = 3,

        /// <summary>
        /// TOIC
        /// </summary>
        TOIC = 4,

        /// <summary>
        /// TOJTAG
        /// </summary>
        TOJTAG = 5,

        /// <summary>
        /// JTAG
        /// </summary>
        JTAG = 6,

        /// <summary>
        /// Directly connected to GND nets
        /// </summary>
        GND,

        /// <summary>
        /// directly connected to power nets
        /// </summary>
        POWER,

        /// <summary>
        /// if not another PINTYPE could be determined
        /// </summary>
        INVALID,
    }

    public enum BoundaryScanTestTypeInternal
    {
        /// <summary>
        /// Pullup
        /// </summary>
        Pullup = 0,

        /// <summary>
        /// Pulldown
        /// </summary>
        Pulldown = 1,

        /// <summary>
        /// PullupPulldown
        /// </summary>
        PullupPulldown = 2,

        /// <summary>
        /// UnknownControl
        /// </summary>
        UnknownControl = 3,

        /// <summary>
        /// UnknownInput
        /// </summary>
        UnknownInput = 4,

        /// <summary>
        /// DirectGnd
        /// </summary>
        DirectGnd = 5,

        /// <summary>
        /// DirectPower
        /// </summary>
        DirectPower = 6,

        /// <summary>
        /// Neighbouring
        /// </summary>
        Neighbouring = 7,
    }

    public class PinConnectionTypeContainer
    {
        public PinConnectionType PinConnectionType { get; set; }

        public List<BoundaryScanTestTypeInternal> BoundaryScanTypes { get; set; } = new List<BoundaryScanTestTypeInternal>();

        public List<IPCBComponent> ConnectedComponents { get; set; }

        public override int GetHashCode()
        {
            return PinConnectionType.GetHashCode() + BoundaryScanTypes.GetHashCode() + ConnectedComponents.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PinConnectionTypeContainer container))
            {
                return false;
            }

            return PinConnectionType.Equals(container.PinConnectionType) && BoundaryScanTypes.SequenceEqual(container.BoundaryScanTypes)
                && ConnectedComponents.SequenceEqual(container.ConnectedComponents);
        }
    }
}
