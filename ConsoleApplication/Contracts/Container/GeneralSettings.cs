using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.Console.Contracts.Container
{
    public class GeneralSettings
    {
        public string ResistorIdentifier { get; set; }

        public string CapacitorIdentifier { get; set; }

        public string InductionIdentifer { get; set; }

        public string IcIdentifier { get; set; }

        public string ConnectorIdentifier { get; set; }

        public string TestPointIdentifier { get; set; }

        public bool UseContains { get; set; }
    }
}
