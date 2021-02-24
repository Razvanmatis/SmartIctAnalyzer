using System;
using System.Collections.Generic;
using System.Text;

namespace PinInformationExtractor.JSON
{
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
