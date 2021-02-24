using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.UnitTests
{
    public class PinTestObject
    {
        public PinTestObject(string pinNumber, List<string> nets)
        {
            PinNumber = pinNumber;
            Nets = nets;
        }

        public PinTestObject()
        {
        }

        public string PinNumber { get; set; }

        public List<string> Nets { get; set; } = new List<string>();
    }
}
