using System;
using System.Collections.Generic;
using System.Text;

namespace PinInformationExtractor.JSON
{
    public class ConnectedJtagDevice
    {
        public ConnectedJtagDevice(string name, List<string> pinNames)
        {
            Name = name;
            PinNames = pinNames;
        }

        public string Name { get; set; }
        public List<string> PinNames { get; set; } 
    }
}
