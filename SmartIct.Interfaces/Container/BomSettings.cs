using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.Interfaces.Container
{
    public class BomSettings
    {
        public BomSettings(string separator, string colRef, string colValue)
        {
            Separator = separator;
            ColRef = colRef;
            ColValue = colValue;
        }

        public string Separator { get; set; }

        public string ColRef { get; set; }

        public string ColValue { get; set; }
    }
}
