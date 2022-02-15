using System.Collections.Generic;
using ProMik.SmartIct.Interfaces.Helper;

namespace ProMik.SmartIct.Interfaces.Container
{
    public class PinConnectionInfo
    {
        public PinConnectionInfo(
            string pinNumber,
            List<PinConnectionTypeContainer> pinConnections,
            List<string> netNames,
            List<ConnectedJtagDevice> connectedJtag = null)
        {
            PinNumber = pinNumber;
            PinConnections = pinConnections;
            ConnectedJtagDevice = connectedJtag;
            NetNames = netNames;
        }

        public string PinNumber { get; set; }

        public List<string> NetNames { get; set; }

        public List<PinConnectionTypeContainer> PinConnections { get; set; }

        public List<ConnectedJtagDevice> ConnectedJtagDevice { get; set; }
    }
}
