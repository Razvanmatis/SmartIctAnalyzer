using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Model
{
    public class SettingsStorageModel : ISettingsStorageModel
    {
        public SettingsStorageModel(
            string r,
            string c,
            string i,
            string t,
            string ict,
            string cont,
            int font,
            StorageColor rc,
            StorageColor cc,
            StorageColor ic,
            StorageColor tc,
            StorageColor icc,
            StorageColor compC,
            StorageColor conC,
            bool rShow,
            bool cShow,
            bool iShow,
            bool tpShow,
            bool icShow,
            bool compShow,
            bool conShow,
            bool netsShow,
            string gndIdent,
            string powerIdent,
            string jtagIdent,
            string gndBlacklist,
            string powerBlacklist,
            string jtagBlacklist,
            string iPAddress,
            bool showButtons,
            string pgmIp,
            uint pgmPort,
            uint supplyVoltageMv,
            uint ioVoltageMv,
            string steps)
        {
            RIdentifier = r;
            RColor = rc;
            CIdentifier = c;
            CColor = cc;
            IIdentifier = i;
            IColor = ic;
            TIdentifier = t;
            TColor = tc;
            ICIdentifier = ict;
            ICColor = icc;
            ConIdentifier = cont;
            ConColor = conC;
            Fontsize = font;
            CompColor = compC;
            RShow = rShow;
            CShow = cShow;
            IShow = iShow;
            TPShow = tpShow;
            ICShow = icShow;
            CompShow = compShow;
            ConShow = conShow;
            NetsShow = netsShow;
            JTAGNetIdentifier = jtagIdent;
            PowerNetIdentifier = powerIdent;
            GndNetIdentifier = gndIdent;
            JTAGNetBlacklist = jtagBlacklist;
            PowerNetBlacklist = powerBlacklist;
            GndNetBlacklist = gndBlacklist;
            Ip = iPAddress;
            ShowButtons = showButtons;
            PgmIp = pgmIp;
            PgmPort = pgmPort;
            SupplyVoltageMv = supplyVoltageMv;
            IoVoltageMv = ioVoltageMv;
            Steps = steps;
        }

        public bool RShow { get; set; }

        public bool CShow { get; set; }

        public bool IShow { get; set; }

        public bool TPShow { get; set; }

        public bool ICShow { get; set; }

        public bool CompShow { get; set; }

        public bool ConShow { get; set; }

        public bool NetsShow { get; set; }

        public string RIdentifier { get; set; }

        public StorageColor RColor { get; set; }

        public string CIdentifier { get; set; }

        public StorageColor CColor { get; set; }

        public string IIdentifier { get; set; }

        public StorageColor IColor { get; set; }

        public string TIdentifier { get; set; }

        public StorageColor TColor { get; set; }

        public string ICIdentifier { get; set; }

        public StorageColor ICColor { get; set; }

        public string ConIdentifier { get; set; }

        public StorageColor ConColor { get; set; }

        public int Fontsize { get; set; }

        public StorageColor CompColor { get; set; }

        public string JTAGNetIdentifier { get; set; }

        public string JTAGNetBlacklist { get; set; }

        public string PowerNetIdentifier { get; set; }

        public string PowerNetBlacklist { get; set; }

        public string GndNetIdentifier { get; set; }

        public string GndNetBlacklist { get; set; }

        public string Ip { get; set; }

        public bool ShowButtons { get; set; }

        public string PgmIp { get; set; }

        public uint PgmPort { get; set; }

        public uint SupplyVoltageMv { get; set; }

        public uint IoVoltageMv { get; set; }

        public string Steps { get; set; }
    }
}
