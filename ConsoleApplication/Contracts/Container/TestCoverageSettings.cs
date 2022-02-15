using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.Console.Contracts.Container
{
    public class TestCoverageSettings
    {
        public string GndIdentifier { get; set; }

        public string GndBlacklist { get; set; }

        public string PowerIdentifier { get; set; }

        public string PowerBlacklist { get; set; }

        public string JtagIdentifier { get; set; }

        public string JtagBlacklist { get; set; }
    }
}
