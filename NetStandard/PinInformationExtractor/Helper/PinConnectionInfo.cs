using System.Collections.Generic;

namespace PinInformationExtractor.Helper
{
    public enum PinConnectionType
    {
        /// <summary>
        /// UNKNOWN
        /// </summary>
        UNKNOWN = 0,

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
    }

    public class PinConnectionInfo
    {
        public PinConnectionInfo(string pinNumber, List<PinConnectionType> pinConnectionType, List<string> netNames, List<ConnectedJtagDevice> connectedJtag = null)
        {
            PinNumber = pinNumber;
            PinConnectionType = pinConnectionType;
            ConnectedJtagDevice = connectedJtag;
            NetNames = netNames;
        }

        public string PinNumber { get; set; }

        public List<string> NetNames { get; set; }

        public List<PinConnectionType> PinConnectionType { get; set; }

        public List<ConnectedJtagDevice> ConnectedJtagDevice { get; set; }
    }
}
