using System;
using System.Collections.Generic;
using System.Text;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Model
{
    public class BomDataModel : IBomDataModel
    {
        public string ColumnRef { get; set; }

        public string ColumnValue { get; set; }

        public string BomFile { get; set; }

        public string Separator { get; set; }

        public void ResetValues()
        {
            ColumnRef = string.Empty;
            ColumnValue = string.Empty;
            BomFile = string.Empty;
            Separator = string.Empty;
        }
    }
}
