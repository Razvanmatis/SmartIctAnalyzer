using ProMik.SmartIct.Interfaces.Container;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Model
{
    public class BomDataModel : IBomDataModel
    {
        public string ColumnRef { get; set; }

        public string ColumnValue { get; set; }

        public string BomFile { get; set; }

        public string Separator { get; set; }

        public string BomData { get; set; }

        public BomSettings BomSettings { get; set; }

        public void SetBomSettings(BomSettings settings)
        {
            ColumnRef = settings.ColRef;
            ColumnValue = settings.ColValue;
            Separator = settings.Separator;
            BomSettings = settings;
        }

        public void ResetValues()
        {
            ColumnRef = string.Empty;
            ColumnValue = string.Empty;
            BomFile = string.Empty;
            Separator = string.Empty;
            BomSettings = null;
            BomData = string.Empty;
        }
    }
}
