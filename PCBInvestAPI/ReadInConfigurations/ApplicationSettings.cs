using System;
using System.Collections.Generic;
using System.Text;

namespace PCBInvestAPI.ReadInConfigurations
{
    public struct ApplicationSettings
    {
        public bool PCBInvestigatorAvailable { get; set; }
        public string PathtoPCBExe { get; set; }
        public bool ExcelWorks { get; set; }
    }
}
