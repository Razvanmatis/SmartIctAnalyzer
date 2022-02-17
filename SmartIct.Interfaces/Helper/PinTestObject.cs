using System.Collections.Generic;
using System.Linq;

namespace ProMik.SmartIct.Interfaces.Helper
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

        public override int GetHashCode()
        {
            return PinNumber.GetHashCode() + Nets.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is PinTestObject container))
            {
                return false;
            }

            return PinNumber.Equals(container.PinNumber) && Nets.SequenceEqual(container.Nets);
        }
    }
}
