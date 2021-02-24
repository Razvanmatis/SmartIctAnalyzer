using Interfaces.PcbInvestigator;
using System;
using System.Collections.Generic;
using System.Text;

namespace PinInformationExtractor
{
    public enum PinConnectionType
    {
        UNKNOWN = 0,
        PULLUP = 1,
        PULLDOWN = 2,
        NOTCONNECTED = 3,
        TOIC = 4,
        TOJTAG = 5,
        JTAG = 6,
    }

    public interface IPinInformationExtractor
    {
        bool CreatePinInformationFile(string filePath, IList<IPCBComponent> ics, IList<IPCBComponent> pullUps, IList<IPCBComponent> pullDowns,
            string tdiIdentifier = "tdi", string tdoIdentifier = "tdo", string tckIdentifier = "tck", string tmsIdentifier = "tms");
    }
}
