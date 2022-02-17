using System.Collections.Generic;
using System.Windows.Media;
using ProMik.SmartIct.Interfaces.Gui;
using Ui.Modules.ModuleName.Implementations;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface ISettingsData : IBomUseValues
    {
        bool UseContains { get; set; }

        IList<string> ResistorChars { get; set; }

        Color ResistorColor { get; set; }

        IList<string> CapacitorChars { get; set; }

        string IPAddress { get; set; }

        Color CapacitorColor { get; set; }

        IList<string> InductionChars { get; set; }

        Color InductionColor { get; set; }

        IList<string> TestpointChars { get; set; }

        Color TestpointColor { get; set; }

        IList<string> ICChars { get; set; }

        Color ICColor { get; set; }

        IList<string> ConnectorChars { get; set; }

        Color ConnectorColor { get; set; }

        Color CompColor { get; set; }

        int FontSizeNames { get; set; }

        bool ResistorNamesChecked { get; set; }

        bool CapacitorNamesChecked { get; set; }

        bool InductionNamesChecked { get; set; }

        bool TestpointNamesChecked { get; set; }

        bool ICNamesChecked { get; set; }

        bool CompNamesChecked { get; set; }

        bool ConnectorNamesChecked { get; set; }

        bool NetNamesChecked { get; set; }

        public string JTAGPinIdentifier { get; set; }

        public string JTAGNetBlacklist { get; set; }

        public string PowerNetIdentifier { get; set; }

        public string PowerNetBlacklist { get; set; }

        public string GndNetIdentifier { get; set; }

        public string GndNetBlacklist { get; set; }

        bool ShowButtons { get; set; }

        string PgmIp { get; set; }

        uint PgmPort { get; set; }

        uint SupplyVoltageMv { get; set; }

        uint IoVoltageMv { get; set; }

        uint Frequency { get; set; }

        uint CableCompensation { get; set; }

        string Steps { get; set; }

        bool AskForSvfSettings { get; set; }

        TargetDef Target { get; set; }

        SlotDef Slot { get; set; }

        void InitContent();

        void SaveValues();
    }
}
