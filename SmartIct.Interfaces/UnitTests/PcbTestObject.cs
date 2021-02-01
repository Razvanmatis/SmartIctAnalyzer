using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.UnitTests
{
    public class PcbTestObject
    {
        public PcbTestObject()
        {
        }

        public PcbTestObject(string name, List<PinTestObject> pins)
        {
            Name = name;
            Pins = pins;
        }

        public List<PinTestObject> Pins { get; set; } = new List<PinTestObject>();

        public string Name { get; set; }
    }
}
