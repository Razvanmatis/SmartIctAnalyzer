using System.Collections.Generic;
using System.Windows.Media;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.Model;

namespace Ui.Modules.ModuleName.Implementations
{
    public class JsonSettingsData : ISettingsData
    {
        private readonly ISettingsStorageManager storageManager;
        private ISettingsStorageModel content;

        public JsonSettingsData(ISettingsStorageManager storageManager)
        {
            this.storageManager = storageManager;
            InitContent();
        }

        public IList<string> ResistorChars { get; set; }

        public Color ResistorColor { get; set; }

        public IList<string> CapacitorChars { get; set; }

        public Color CapacitorColor { get; set; }

        public IList<string> InductionChars { get; set; }

        public Color InductionColor { get; set; }

        public IList<string> TestpointChars { get; set; }

        public Color TestpointColor { get; set; }

        public IList<string> ICChars { get; set; }

        public Color ICColor { get; set; }

        public IList<string> ConnectorChars { get; set; }

        public Color ConnectorColor { get; set; }

        public Color CompColor { get; set; }

        public int FontSizeNames { get; set; }

        public bool ResistorNamesChecked { get; set; }

        public bool CapacitorNamesChecked { get; set; }

        public bool InductionNamesChecked { get; set; }

        public bool TestpointNamesChecked { get; set; }

        public bool ICNamesChecked { get; set; }

        public bool CompNamesChecked { get; set; }

        public bool ConnectorNamesChecked { get; set; }

        public bool NetNamesChecked { get; set; }

        public bool UseValues { get; set; }

        public string JTAGNetIdentifier { get; set; }

        public string JTAGNetBlacklist { get; set; }

        public string PowerNetIdentifier { get; set; }

        public string PowerNetBlacklist { get; set; }

        public string GndNetIdentifier { get; set; }

        public string GndNetBlacklist { get; set; }

        public string IPAddress { get; set; }

        public bool ShowButtons { get; set; }

        public string PgmIp { get; set; }

        public uint PgmPort { get; set; }

        public uint SupplyVoltageMv { get; set; }

        public uint IoVoltageMv { get; set; }

        public string Steps { get; set; }

        public void InitContent()
        {
            content = storageManager.GetStorageContent();
            ResistorColor = Color.FromRgb((byte)content.RColor.R, (byte)content.RColor.G, (byte)content.RColor.B);
            CapacitorColor = Color.FromRgb((byte)content.CColor.R, (byte)content.CColor.G, (byte)content.CColor.B);
            InductionColor = Color.FromRgb((byte)content.IColor.R, (byte)content.IColor.G, (byte)content.IColor.B);
            TestpointColor = Color.FromRgb((byte)content.TColor.R, (byte)content.TColor.G, (byte)content.TColor.B);
            ICColor = Color.FromRgb((byte)content.ICColor.R, (byte)content.ICColor.G, (byte)content.ICColor.B);
            CompColor = Color.FromRgb((byte)content.CompColor.R, (byte)content.CompColor.G, (byte)content.CompColor.B);
            ConnectorColor = Color.FromRgb((byte)content.ConColor.R, (byte)content.ConColor.G, (byte)content.ConColor.B);
            ResistorChars = GetListContent(content.RIdentifier);
            CapacitorChars = GetListContent(content.CIdentifier);
            InductionChars = GetListContent(content.IIdentifier);
            TestpointChars = GetListContent(content.TIdentifier);
            ICChars = GetListContent(content.ICIdentifier);
            ConnectorChars = GetListContent(content.ConIdentifier);
            FontSizeNames = content.Fontsize;
            ResistorNamesChecked = content.RShow;
            CapacitorNamesChecked = content.CShow;
            InductionNamesChecked = content.IShow;
            TestpointNamesChecked = content.TPShow;
            ConnectorNamesChecked = content.ConShow;
            ICNamesChecked = content.ICShow;
            CompNamesChecked = content.CompShow;
            NetNamesChecked = content.NetsShow;
            JTAGNetIdentifier = content.JTAGNetIdentifier;
            JTAGNetBlacklist = content.JTAGNetBlacklist;
            PowerNetIdentifier = content.PowerNetIdentifier;
            PowerNetBlacklist = content.PowerNetBlacklist;
            GndNetIdentifier = content.GndNetIdentifier;
            GndNetBlacklist = content.GndNetBlacklist;
            IPAddress = content.Ip;
            ShowButtons = content.ShowButtons;
            PgmIp = content.PgmIp;
            PgmPort = content.PgmPort;
            SupplyVoltageMv = content.SupplyVoltageMv;
            IoVoltageMv = content.IoVoltageMv;
            Steps = content.Steps;
        }

        public void SaveValues()
        {
            content = new SettingsStorageModel(
                GetTextFromList(ResistorChars),
                GetTextFromList(CapacitorChars),
                GetTextFromList(InductionChars),
                GetTextFromList(TestpointChars),
                GetTextFromList(ICChars),
                GetTextFromList(ConnectorChars),
                FontSizeNames,
                new StorageColor(ResistorColor.R, ResistorColor.G, ResistorColor.B),
                new StorageColor(CapacitorColor.R, CapacitorColor.G, CapacitorColor.B),
                new StorageColor(InductionColor.R, InductionColor.G, InductionColor.B),
                new StorageColor(TestpointColor.R, TestpointColor.G, TestpointColor.B),
                new StorageColor(ICColor.R, ICColor.G, ICColor.B),
                new StorageColor(CompColor.R, CompColor.G, CompColor.B),
                new StorageColor(ConnectorColor.R, ConnectorColor.G, ConnectorColor.B),
                ResistorNamesChecked,
                CapacitorNamesChecked,
                InductionNamesChecked,
                TestpointNamesChecked,
                ICNamesChecked,
                CompNamesChecked,
                ConnectorNamesChecked,
                NetNamesChecked,
                GndNetIdentifier,
                PowerNetIdentifier,
                JTAGNetIdentifier,
                GndNetBlacklist,
                PowerNetBlacklist,
                JTAGNetBlacklist,
                IPAddress,
                ShowButtons,
                PgmIp,
                PgmPort,
                SupplyVoltageMv,
                IoVoltageMv,
                Steps);
            storageManager.SaveStorageContent(content);
        }

        private static IList<string> GetListContent(string value)
        {
            IList<string> list = new List<string>();
            foreach (var text in value.Split(";"))
            {
                list.Add(text);
            }

            return list;
        }

        private static string GetTextFromList(IList<string> values)
        {
            string text = string.Empty;
            foreach (string val in values)
            {
                text += val + ";";
            }

            return text[0..^1];
        }
    }
}
