using System.Collections.Generic;
using ProMik.SmartIct.Interfaces.PcbInvestigator;

namespace ProMik.SmartIct.Interfaces.Container
{
    public class ConnectedJtagDevice
    {
        public ConnectedJtagDevice(IPCBComponent jtag, string name, List<string> pinNames)
        {
            Name = name;
            PinNames = pinNames;
            JTAG = jtag;
        }

        public string Name { get; set; }

        public List<string> PinNames { get; set; }

        public IPCBComponent JTAG { get; set; }
    }
}
