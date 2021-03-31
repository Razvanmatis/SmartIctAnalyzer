using Ui.Modules.ModuleName.Helper;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IBomDataModel
    {
        string ColumnRef { get; set; }

        string ColumnValue { get; set; }

        string BomFile { get; set; }

        string Separator { get; set; }

        string BomData { get; set; }

        BomSettings BomSettings { get; set; }

        void ResetValues();

        void SetBomSettings(BomSettings settings);
    }
}
