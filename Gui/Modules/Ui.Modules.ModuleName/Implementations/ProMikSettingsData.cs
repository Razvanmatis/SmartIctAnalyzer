using System.Collections.Generic;
using System.Windows.Media;
using ProMik.Core.Interfaces.Settings;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class ProMikSettingsData : ISettingsData
    {
        private const string PAGEKEYGENERAL = "pageKeyGeneral";
        private const string PAGENAMEGENERAL = "General settings";
        private const string SECTIONRKEY = "sectionRKey";
        private const string SECTIONRNAME = "Resistors settings";
        private const string RCHARSSETTINGKEY = "rCharsSettingKey";
        private const string RCHARSETTINGSNAME = "Resistor identifier (a;b;c):";
        private const string RCOLORSETTINGKEY = "rColorSettingKey";
        private const string RCOLORSETTINGSNAME = "Resistor color:";
        private const string RCHECKNAMESETTINGKEY = "rCheckNameSettingKey";
        private const string SHOWNAMES = "Show names";
        private const string SECTIONCKEY = "sectionCKey";
        private const string SECTIONCNAME = "Capacitor settings";
        private const string CCHARSSETTINGKEY = "cCharsSettingKey";
        private const string CCHARSETTINGSNAME = "Capacitor identifier (a;b;c):";
        private const string CCOLORSETTINGKEY = "cColorSettingKey";
        private const string CCOLORSETTINGSNAME = "Capacitor color:";
        private const string CCHECKNAMESETTINGKEY = "cCheckNameSettingKey";
        private const string SECTIONIKEY = "sectionIKey";
        private const string SECTIONINAME = "Induction settings";
        private const string ICHARSSETTINGKEY = "iCharsSettingKey";
        private const string ICHARSETTINGSNAME = "Induction identifier (a;b;c):";
        private const string ICOLORSETTINGKEY = "iColorSettingKey";
        private const string ICOLORSETTINGSNAME = "Induction color:";
        private const string ICHECKNAMESETTINGKEY = "iCheckNameSettingKey";
        private const string SECTIONICKEY = "sectionIcKey";
        private const string SECTIONICNAME = "IC settings";
        private const string ICCHARSSETTINGKEY = "icCharsSettingKey";
        private const string ICCHARSETTINGSNAME = "IC identifier (a;b;c):";
        private const string ICCOLORSETTINGKEY = "icColorSettingKey";
        private const string ICCOLORSETTINGSNAME = "IC color:";
        private const string ICCHECKNAMESETTINGKEY = "icCheckNameSettingKey";
        private const string SECTIONCONKEY = "sectionConKey";
        private const string SECTIONCONNAME = "Connector settings";
        private const string CONCHARSSETTINGKEY = "conCharsSettingKey";
        private const string CONCHARSETTINGSNAME = "Connector identifier (a;b;c):";
        private const string CONCOLORSETTINGKEY = "conColorSettingKey";
        private const string CONCOLORSETTINGSNAME = "Connector color:";
        private const string CONCHECKNAMESETTINGKEY = "conCheckNameSettingKey";
        private const string SECTIONTPKEY = "sectionTPKey";
        private const string SECTIONTPNAME = "Testpoint settings";
        private const string TPCHARSSETTINGKEY = "tpCharsSettingKey";
        private const string TPCHARSETTINGSNAME = "Testpoint identifier (a;b;c):";
        private const string TPCOLORSETTINGKEY = "tpColorSettingKey";
        private const string TPCOLORSETTINGSNAME = "Testpoint color:";
        private const string TPCHECKNAMESETTINGKEY = "tpCheckNameSettingKey";
        private const string SECTIONCOMPKEY = "sectionCOMPKey";
        private const string SECTIONCOMPNAME = "All other components settings";
        private const string COMPCOLORSETTINGKEY = "compColorSettingKey";
        private const string COMPCOLORSETTINGSNAME = "Color of all other components:";
        private const string COMPCHECKNAMESETTINGKEY = "compCheckNameSettingKey";
        private const string SECTIONGENKEY = "sectionGenKey";
        private const string SECTIONGENNAME = "General appearance";
        private const string FONTSIZESETTINGKEY = "fontSizeSettingKey";
        private const string FONTSIZESETTINGNAME = "Fontsize of names:";
        private const string SHOWNETNAMESSETTINGKEY = "showNetNamesSettingKey";
        private const string SHOWNETNAMESSETTINGNAME = "Show net names";
        private const string SHOWBUTTONSSETTINGKEY = "showButtonsSettingKey";
        private const string SHOWBUTTONSSETTINGNAME = "Show zoom and mirroring buttons";
        private const string PAGEKEYAPI = "pageKeyApi";
        private const string PAGENAMEAPI = "PCB Investigator API settings";
        private const string SECTIONAPIKEY = "sectionApiKey";
        private const string SECTIONAPINAME = PAGENAMEAPI;
        private const string STEPSSETTINGKEY = "stepsSettingKey";
        private const string STEPSSETTINGNAME = "Steps to be read by PCB Investigator API (a;b;c)(Beginning with 1, -1 = all):";
        private const string PAGEKEYCONNECTION = "pageKeyConnection";
        private const string PAGENAMECONNECTION = "GRPC server API connection settings";
        private const string CONNECTIONSECTIONKEY = "connectionSectionKey";
        private const string IPSETTINGKEY = "ipSettingKey";
        private const string IPSETTINGNAME = "IP address of GRPC API server:";
        private const string CONNECTIONSECTIONNAME = PAGENAMECONNECTION;
        private const string PAGEKEYTESTCOVERAGE = "pageKeyTestCoverage";
        private const string PAGENAMETESTCOVERAGE = "Test coverage settings";
        private const string GNDSECTIONKEY = "gndSectionKey";
        private const string GNDSECTIONNAME = "GND nets settings";
        private const string GNDIDENTSETTINGKEY = "gndIdentSettingKey";
        private const string GNDIDENTSETTINGNAME = "GND nets identifier (a;b;c):";
        private const string GNDBLACKSETTINGKEY = "gndBlackSettingKey";
        private const string BLACKLISTNAME = "Blacklist (a;b;c):";
        private const string POWERSECTIONKEY = "powerSectionKey";
        private const string POWERSECTIONNAME = "Power nets settings";
        private const string POWERIDENTSETTINGKEY = "POWERIdentSettingKey";
        private const string POWERIDENTSETTINGNAME = "Power nets identifier (a;b;c):";
        private const string POWERBLACKSETTINGKEY = "POWERBlackSettingKey";
        private const string JTAGSECTIONKEY = "JTAGSectionKey";
        private const string JTAGSECTIONNAME = "JTAG nets settings";
        private const string JTAGIDENTSETTINGKEY = "JTAGIdentSettingKey";
        private const string JTAGIDENTSETTINGNAME = "JTAG nets identifier (a;b;c):";
        private const string JTAGBLACKSETTINGKEY = "JTAGBlackSettingKey";
        private const string PAGEKEYSVF = "pageKeySvf";
        private const string PAGENAMESVF = "SVF player settings";
        private const string SECTIONSVFKEY = "sectionSvfProgrammerKey";
        private const string SECTIONSVFNAME = "Programmer connection settings";
        private const string SVFPGMSETTINGKEY = "svfPgmSettingKey";
        private const string SVFPGMNAME = "Programmer IP:";
        private const string SVFPORTSETTINGKEY = "svfPortSettingKey";
        private const string SVFPORTNAME = "Programmer Port:";
        private const string SVFSUPPLYVSETTINGKEY = "svfSupplyVoltageSettingKey";
        private const string SVFSUPPLYVNAME = "Supply Voltage in mv:";
        private const string SVFIOVOLTAGESETTINGKEY = "svfIoVoltageSettingKey";
        private const string SVFIOVOLTAGENAME = "IO Voltage in mv:";
        private readonly ISettingsService settingsService;
        private IList<string> resistorChars;
        private Color resistorColor;
        private bool resistorNamesChecked;
        private IList<string> capacitorChars;
        private Color capacitorColor;
        private bool capacitorNamesChecked;
        private IList<string> inductionChars;
        private Color inductionColor;
        private bool inductionNamesChecked;
        private IList<string> icChars;
        private Color icColor;
        private bool icNamesChecked;
        private IList<string> connectorChars;
        private Color connectorColor;
        private bool connectorNamesChecked;
        private IList<string> testpointChars;
        private Color testpointColor;
        private bool testpointNamesChecked;
        private Color compColor;
        private bool compNamesChecked;
        private int fontSizeNames;
        private bool netNamesChecked;
        private bool showButtons;
        private string steps;
        private string ipAddress;
        private string jtagNetIdentifier;
        private string jtagNetBlacklist;
        private string powerNetIdentifier;
        private string powerNetBlacklist;
        private string gndNetIdentifier;
        private string gndNetBlacklist;
        private string pgmIp;
        private uint pgmPort;
        private uint supplyVoltageMv;
        private uint ioVoltageMv;

        public ProMikSettingsData(ISettingsService settingsService)
        {
            this.settingsService = settingsService;
            DefineSettings();
            InitContent();
        }

        public IList<string> ResistorChars
        {
            get
            {
                return resistorChars;
            }

            set
            {
            }
        }

        public Color ResistorColor
        {
            get
            {
                return resistorColor;
            }

            set
            {
            }
        }

        public bool ResistorNamesChecked
        {
            get
            {
                return resistorNamesChecked;
            }

            set
            {
            }
        }

        public IList<string> CapacitorChars
        {
            get
            {
                return capacitorChars;
            }

            set
            {
            }
        }

        public Color CapacitorColor
        {
            get
            {
                return capacitorColor;
            }

            set
            {
            }
        }

        public bool CapacitorNamesChecked
        {
            get
            {
                return capacitorNamesChecked;
            }

            set
            {
            }
        }

        public IList<string> InductionChars
        {
            get
            {
                return inductionChars;
            }

            set
            {
            }
        }

        public Color InductionColor
        {
            get
            {
                return inductionColor;
            }

            set
            {
            }
        }

        public bool InductionNamesChecked
        {
            get
            {
                return inductionNamesChecked;
            }

            set
            {
            }
        }

        public IList<string> ICChars
        {
            get
            {
                return icChars;
            }

            set
            {
            }
        }

        public Color ICColor
        {
            get
            {
                return icColor;
            }

            set
            {
            }
        }

        public bool ICNamesChecked
        {
            get
            {
                return icNamesChecked;
            }

            set
            {
            }
        }

        public IList<string> ConnectorChars
        {
            get
            {
                return connectorChars;
            }

            set
            {
            }
        }

        public Color ConnectorColor
        {
            get
            {
                return connectorColor;
            }

            set
            {
            }
        }

        public bool ConnectorNamesChecked
        {
            get
            {
                return connectorNamesChecked;
            }

            set
            {
            }
        }

        public IList<string> TestpointChars
        {
            get
            {
                return testpointChars;
            }

            set
            {
            }
        }

        public Color TestpointColor
        {
            get
            {
                return testpointColor;
            }

            set
            {
            }
        }

        public bool TestpointNamesChecked
        {
            get
            {
                return testpointNamesChecked;
            }

            set
            {
            }
        }

        public Color CompColor
        {
            get
            {
                return compColor;
            }

            set
            {
            }
        }

        public bool CompNamesChecked
        {
            get
            {
                return compNamesChecked;
            }

            set
            {
            }
        }

        public int FontSizeNames
        {
            get
            {
                return fontSizeNames;
            }

            set
            {
            }
        }

        public bool NetNamesChecked
        {
            get
            {
                return netNamesChecked;
            }

            set
            {
            }
        }

        public bool ShowButtons
        {
            get
            {
                return showButtons;
            }

            set
            {
            }
        }

        public string Steps
        {
            get
            {
                return steps;
            }

            set
            {
            }
        }

        public string IPAddress
        {
            get
            {
                return ipAddress;
            }

            set
            {
            }
        }

        public string JTAGNetIdentifier
        {
            get
            {
                return jtagNetIdentifier;
            }

            set
            {
            }
        }

        public string JTAGNetBlacklist
        {
            get
            {
                return jtagNetBlacklist;
            }

            set
            {
            }
        }

        public string PowerNetIdentifier
        {
            get
            {
                return powerNetIdentifier;
            }

            set
            {
            }
        }

        public string PowerNetBlacklist
        {
            get
            {
                return powerNetBlacklist;
            }

            set
            {
            }
        }

        public string GndNetIdentifier
        {
            get
            {
                return gndNetIdentifier;
            }

            set
            {
            }
        }

        public string GndNetBlacklist
        {
            get
            {
                return gndNetBlacklist;
            }

            set
            {
            }
        }

        public string PgmIp
        {
            get
            {
                return pgmIp;
            }

            set
            {
            }
        }

        public uint PgmPort
        {
            get
            {
                return pgmPort;
            }

            set
            {
            }
        }

        public uint SupplyVoltageMv
        {
            get
            {
                return supplyVoltageMv;
            }

            set
            {
            }
        }

        public uint IoVoltageMv
        {
            get
            {
                return ioVoltageMv;
            }

            set
            {
            }
        }

        public bool UseValues { get; set; }

        public void InitContent()
        {
            settingsService.Reinitialize();
            resistorChars = GetListContent(settingsService.GetSettingValue<string>(RCHARSSETTINGKEY));
            resistorColor = (Color)ColorConverter.ConvertFromString(settingsService.GetSettingValue<string>(RCOLORSETTINGKEY));
            resistorNamesChecked = settingsService.GetSettingValue<bool>(RCHECKNAMESETTINGKEY);
            capacitorChars = GetListContent(settingsService.GetSettingValue<string>(CCHARSSETTINGKEY));
            capacitorColor = (Color)ColorConverter.ConvertFromString(settingsService.GetSettingValue<string>(CCOLORSETTINGKEY));
            capacitorNamesChecked = settingsService.GetSettingValue<bool>(CCHECKNAMESETTINGKEY);
            inductionChars = GetListContent(settingsService.GetSettingValue<string>(ICHARSSETTINGKEY));
            inductionColor = (Color)ColorConverter.ConvertFromString(settingsService.GetSettingValue<string>(ICOLORSETTINGKEY));
            inductionNamesChecked = settingsService.GetSettingValue<bool>(ICHECKNAMESETTINGKEY);
            icChars = GetListContent(settingsService.GetSettingValue<string>(ICCHARSSETTINGKEY));
            icColor = (Color)ColorConverter.ConvertFromString(settingsService.GetSettingValue<string>(ICCOLORSETTINGKEY));
            icNamesChecked = settingsService.GetSettingValue<bool>(ICCHECKNAMESETTINGKEY);
            connectorChars = GetListContent(settingsService.GetSettingValue<string>(CONCHARSSETTINGKEY));
            connectorColor = (Color)ColorConverter.ConvertFromString(settingsService.GetSettingValue<string>(CONCOLORSETTINGKEY));
            connectorNamesChecked = settingsService.GetSettingValue<bool>(CONCHECKNAMESETTINGKEY);
            testpointChars = GetListContent(settingsService.GetSettingValue<string>(TPCHARSSETTINGKEY));
            testpointColor = (Color)ColorConverter.ConvertFromString(settingsService.GetSettingValue<string>(TPCOLORSETTINGKEY));
            testpointNamesChecked = settingsService.GetSettingValue<bool>(TPCHECKNAMESETTINGKEY);
            compColor = (Color)ColorConverter.ConvertFromString(settingsService.GetSettingValue<string>(COMPCOLORSETTINGKEY));
            compNamesChecked = settingsService.GetSettingValue<bool>(COMPCHECKNAMESETTINGKEY);
            fontSizeNames = settingsService.GetSettingValue<int>(FONTSIZESETTINGKEY);
            netNamesChecked = settingsService.GetSettingValue<bool>(SHOWNETNAMESSETTINGKEY);
            showButtons = settingsService.GetSettingValue<bool>(SHOWBUTTONSSETTINGKEY);
            steps = settingsService.GetSettingValue<string>(STEPSSETTINGKEY);
            ipAddress = settingsService.GetSettingValue<string>(IPSETTINGKEY);
            jtagNetIdentifier = settingsService.GetSettingValue<string>(JTAGIDENTSETTINGKEY);
            jtagNetBlacklist = settingsService.GetSettingValue<string>(JTAGBLACKSETTINGKEY);
            powerNetIdentifier = settingsService.GetSettingValue<string>(POWERIDENTSETTINGKEY);
            powerNetBlacklist = settingsService.GetSettingValue<string>(POWERBLACKSETTINGKEY);
            gndNetIdentifier = settingsService.GetSettingValue<string>(GNDIDENTSETTINGKEY);
            gndNetBlacklist = settingsService.GetSettingValue<string>(GNDBLACKSETTINGKEY);
            pgmIp = settingsService.GetSettingValue<string>(SVFPGMSETTINGKEY);
            pgmPort = (uint)settingsService.GetSettingValue<int>(SVFPORTSETTINGKEY);
            supplyVoltageMv = (uint)settingsService.GetSettingValue<int>(SVFSUPPLYVSETTINGKEY);
            ioVoltageMv = (uint)settingsService.GetSettingValue<int>(SVFIOVOLTAGESETTINGKEY);
            CheckForNullvalue(ref steps);
            CheckForNullvalue(ref ipAddress);
            CheckForNullvalue(ref jtagNetIdentifier);
            CheckForNullvalue(ref jtagNetBlacklist);
            CheckForNullvalue(ref powerNetIdentifier);
            CheckForNullvalue(ref powerNetBlacklist);
            CheckForNullvalue(ref gndNetIdentifier);
            CheckForNullvalue(ref gndNetBlacklist);
            CheckForNullvalue(ref pgmIp);
        }

        public void SaveValues()
        {
            InitContent();
        }

        private static void CheckForNullvalue(ref string value)
        {
            if (value == null)
            {
                value = string.Empty;
            }
        }

        private static IList<string> GetListContent(string value)
        {
            if (value == null)
            {
                return new List<string>();
            }

            IList<string> list = new List<string>();
            foreach (var text in value.Split(";"))
            {
                list.Add(text);
            }

            return list;
        }

        private static void DefineSvfSettings(IPage svfPage)
        {
            var section = svfPage.AddSection(SECTIONSVFKEY, SECTIONSVFNAME);
            section.AddSetting(SVFPGMSETTINGKEY, SVFPGMNAME, SettingType.String, JsonSettingsStorageManager.PgmIpDef);
            section.AddSetting(SVFPORTSETTINGKEY, SVFPORTNAME, SettingType.Integer, (int)JsonSettingsStorageManager.PgmPortDef);
            section.AddSetting(SVFSUPPLYVSETTINGKEY, SVFSUPPLYVNAME, SettingType.Integer, (int)JsonSettingsStorageManager.SupplyVoltageMvDef);
            section.AddSetting(SVFIOVOLTAGESETTINGKEY, SVFIOVOLTAGENAME, SettingType.Integer, (int)JsonSettingsStorageManager.IoVoltageMvDef);
        }

        private static void DefineTestCoverageSettings(IPage pageTestCoverage)
        {
            var gndSection = pageTestCoverage.AddSection(GNDSECTIONKEY, GNDSECTIONNAME);
            gndSection.AddSetting(GNDIDENTSETTINGKEY, GNDIDENTSETTINGNAME, SettingType.String, JsonSettingsStorageManager.GndDef);
            gndSection.AddSetting(GNDBLACKSETTINGKEY, BLACKLISTNAME, SettingType.String, string.Empty);
            var powerSection = pageTestCoverage.AddSection(POWERSECTIONKEY, POWERSECTIONNAME);
            powerSection.AddSetting(POWERIDENTSETTINGKEY, POWERIDENTSETTINGNAME, SettingType.String, JsonSettingsStorageManager.PowerDef);
            powerSection.AddSetting(POWERBLACKSETTINGKEY, BLACKLISTNAME, SettingType.String, string.Empty);
            var jtagSection = pageTestCoverage.AddSection(JTAGSECTIONKEY, JTAGSECTIONNAME);
            jtagSection.AddSetting(JTAGIDENTSETTINGKEY, JTAGIDENTSETTINGNAME, SettingType.String, JsonSettingsStorageManager.JtagDef);
            jtagSection.AddSetting(JTAGBLACKSETTINGKEY, BLACKLISTNAME, SettingType.String, string.Empty);
        }

        private static void DefineConnectionSettings(IPage pageConnection)
        {
            var section = pageConnection.AddSection(CONNECTIONSECTIONKEY, CONNECTIONSECTIONNAME);
            section.AddSetting(IPSETTINGKEY, IPSETTINGNAME, SettingType.String, JsonSettingsStorageManager.Ipdef);
        }

        private static void DefineApiSettings(IPage pageApi)
        {
            var sectionApi = pageApi.AddSection(SECTIONAPIKEY, SECTIONAPINAME);
            sectionApi.AddSetting(STEPSSETTINGKEY, STEPSSETTINGNAME, SettingType.String, JsonSettingsStorageManager.StepsDef);
        }

        private static void DefineGeneralSettings(IPage pageGeneral)
        {
            var sectionR = pageGeneral.AddSection(SECTIONRKEY, SECTIONRNAME);
            sectionR.AddSetting(RCHARSSETTINGKEY, RCHARSETTINGSNAME, SettingType.String, JsonSettingsStorageManager.RDef);
            sectionR.AddSetting(RCOLORSETTINGKEY, RCOLORSETTINGSNAME, SettingType.Color, JsonSettingsStorageManager.RDefColor.ToString());
            sectionR.AddSetting(RCHECKNAMESETTINGKEY, SHOWNAMES, SettingType.Boolean, false);
            var sectionC = pageGeneral.AddSection(SECTIONCKEY, SECTIONCNAME);
            sectionC.AddSetting(CCHARSSETTINGKEY, CCHARSETTINGSNAME, SettingType.String, JsonSettingsStorageManager.CDef);
            sectionC.AddSetting(CCOLORSETTINGKEY, CCOLORSETTINGSNAME, SettingType.Color, JsonSettingsStorageManager.CDefColor.ToString());
            sectionC.AddSetting(CCHECKNAMESETTINGKEY, SHOWNAMES, SettingType.Boolean, false);
            var sectionI = pageGeneral.AddSection(SECTIONIKEY, SECTIONINAME);
            sectionI.AddSetting(ICHARSSETTINGKEY, ICHARSETTINGSNAME, SettingType.String, JsonSettingsStorageManager.IDef);
            sectionI.AddSetting(ICOLORSETTINGKEY, ICOLORSETTINGSNAME, SettingType.Color, JsonSettingsStorageManager.IDefColor.ToString());
            sectionI.AddSetting(ICHECKNAMESETTINGKEY, SHOWNAMES, SettingType.Boolean, false);
            var sectionIc = pageGeneral.AddSection(SECTIONICKEY, SECTIONICNAME);
            sectionIc.AddSetting(ICCHARSSETTINGKEY, ICCHARSETTINGSNAME, SettingType.String, JsonSettingsStorageManager.IcDef);
            sectionIc.AddSetting(ICCOLORSETTINGKEY, ICCOLORSETTINGSNAME, SettingType.Color, JsonSettingsStorageManager.IcDefColor.ToString());
            sectionIc.AddSetting(ICCHECKNAMESETTINGKEY, SHOWNAMES, SettingType.Boolean, false);
            var sectionCon = pageGeneral.AddSection(SECTIONCONKEY, SECTIONCONNAME);
            sectionCon.AddSetting(CONCHARSSETTINGKEY, CONCHARSETTINGSNAME, SettingType.String, JsonSettingsStorageManager.ConDef);
            sectionCon.AddSetting(CONCOLORSETTINGKEY, CONCOLORSETTINGSNAME, SettingType.Color, JsonSettingsStorageManager.ConDefColor.ToString());
            sectionCon.AddSetting(CONCHECKNAMESETTINGKEY, SHOWNAMES, SettingType.Boolean, true);
            var sectionTp = pageGeneral.AddSection(SECTIONTPKEY, SECTIONTPNAME);
            sectionTp.AddSetting(TPCHARSSETTINGKEY, TPCHARSETTINGSNAME, SettingType.String, JsonSettingsStorageManager.TpDef);
            sectionTp.AddSetting(TPCOLORSETTINGKEY, TPCOLORSETTINGSNAME, SettingType.Color, JsonSettingsStorageManager.TpDefColor.ToString());
            sectionTp.AddSetting(TPCHECKNAMESETTINGKEY, SHOWNAMES, SettingType.Boolean, false);
            var sectionComp = pageGeneral.AddSection(SECTIONCOMPKEY, SECTIONCOMPNAME);
            sectionComp.AddSetting(COMPCOLORSETTINGKEY, COMPCOLORSETTINGSNAME, SettingType.Color, JsonSettingsStorageManager.CompDefColor.ToString());
            sectionComp.AddSetting(COMPCHECKNAMESETTINGKEY, SHOWNAMES, SettingType.Boolean, true);
            var sectionGen = pageGeneral.AddSection(SECTIONGENKEY, SECTIONGENNAME);
            sectionGen.AddSetting(FONTSIZESETTINGKEY, FONTSIZESETTINGNAME, SettingType.Integer, JsonSettingsStorageManager.FontDef);
            sectionGen.AddSetting(SHOWNETNAMESSETTINGKEY, SHOWNETNAMESSETTINGNAME, SettingType.Boolean, false);
            sectionGen.AddSetting(SHOWBUTTONSSETTINGKEY, SHOWBUTTONSSETTINGNAME, SettingType.Boolean, true);
        }

        private void DefineSettings()
        {
            var pageGeneral = settingsService.AddPage(PAGEKEYGENERAL, PAGENAMEGENERAL);
            DefineGeneralSettings(pageGeneral);
            var pageApi = settingsService.AddPage(PAGEKEYAPI, PAGENAMEAPI);
            DefineApiSettings(pageApi);
            var pageConnection = settingsService.AddPage(PAGEKEYCONNECTION, PAGENAMECONNECTION);
            DefineConnectionSettings(pageConnection);
            var pageTestCoverage = settingsService.AddPage(PAGEKEYTESTCOVERAGE, PAGENAMETESTCOVERAGE);
            DefineTestCoverageSettings(pageTestCoverage);
            var svfPage = settingsService.AddPage(PAGEKEYSVF, PAGENAMESVF);
            DefineSvfSettings(svfPage);
        }
    }
}
