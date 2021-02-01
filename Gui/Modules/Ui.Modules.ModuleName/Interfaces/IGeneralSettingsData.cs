using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IGeneralSettingsData
    {
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

        bool UseValues { get; set; }

        public string JTAGNetIdentifier { get; set; }

        public string JTAGNetBlacklist { get; set; }

        public string PowerNetIdentifier { get; set; }

        public string PowerNetBlacklist { get; set; }

        public string GndNetIdentifier { get; set; }

        public string GndNetBlacklist { get; set; }

        void InitContent();

        void SaveValues();
    }
}
