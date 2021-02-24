using System;
using System.Collections.Generic;
using System.Text;

namespace PinInformationExtractor.JSON
{
    public class JtagConnectionInfo
    {
        public JtagConnectionInfo(string componentName, List<PinConnectionInfo> pins, int totalAmountOfNets)
        {
            ComponentName = componentName;
            Pins = pins;
            TotalAmountOfPins = pins.Count;
            TotalAmountOfNets = totalAmountOfNets;
        }

        public string ComponentName { get; set; }
        public int TotalAmountOfPins { get; set; }
        public int TotalAmountOfNets { get; set; }
        public List<PinConnectionInfo> Pins { get; set; }
    }
}
