using System.Collections.Generic;

namespace ProMik.SmartIct.Interfaces.Helper
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
            TotalAmountOfPins = pins.Count;
        }

        public string Name { get; set; }

        public int TotalAmountOfPins { get; set; }

        public List<PinTestObject> Pins { get; set; } = new List<PinTestObject>();
    }
}
