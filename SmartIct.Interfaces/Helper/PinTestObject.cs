using System.Collections.Generic;

namespace Interfaces.Helper
{
    public class PinTestObject
    {
        public PinTestObject(string pinNumber, List<string> nets)
        {
            PinNumber = pinNumber;
            Nets = nets;
        }

        public string PinNumber { get; set; }

        public List<string> Nets { get; set; } = new List<string>();
    }
}
