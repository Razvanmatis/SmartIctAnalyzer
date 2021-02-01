using System;
using System.Collections.Generic;
using System.Text;

namespace Interfaces.UnitTests
{
    public class PinTestObject
    {
        public PinTestObject(List<string> nets)
        {
            Nets = nets;
        }

        public PinTestObject()
        {
        }

        public List<string> Nets { get; set; } = new List<string>();
    }
}
