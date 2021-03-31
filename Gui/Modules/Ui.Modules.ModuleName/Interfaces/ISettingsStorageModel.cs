using Ui.Modules.ModuleName.Helper;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface ISettingsStorageModel
    {
        bool RShow { get; set; }

        bool CShow { get; set; }

        bool IShow { get; set; }

        bool TPShow { get; set; }

        bool ICShow { get; set; }

        bool CompShow { get; set; }

        bool ConShow { get; set; }

        bool NetsShow { get; set; }

        string RIdentifier { get; set; }

        StorageColor RColor { get; set; }

        string CIdentifier { get; set; }

        StorageColor CColor { get; set; }

        string IIdentifier { get; set; }

        StorageColor IColor { get; set; }

        string TIdentifier { get; set; }

        StorageColor TColor { get; set; }

        string ICIdentifier { get; set; }

        StorageColor ICColor { get; set; }

        string ConIdentifier { get; set; }

        StorageColor ConColor { get; set; }

        int Fontsize { get; set; }

        StorageColor CompColor { get; set; }

        string JTAGNetIdentifier { get; set; }

        string JTAGNetBlacklist { get; set; }

        string PowerNetIdentifier { get; set; }

        string PowerNetBlacklist { get; set; }

        string GndNetIdentifier { get; set; }

        string GndNetBlacklist { get; set; }

        string Ip { get; set; }

        bool ShowButtons { get; set; }

        string PgmIp { get; set; }

        uint PgmPort { get; set; }

        uint SupplyVoltageMv { get; }

        uint IoVoltageMv { get; set; }

        string Steps { get; set; }
    }
}
