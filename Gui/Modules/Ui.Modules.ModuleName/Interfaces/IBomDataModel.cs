using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IBomDataModel
    {
        string ColumnRef { get; set; }

        string ColumnValue { get; set; }

        string BomFile { get; set; }

        string Separator { get; set; }

        void ResetValues();
    }
}
