using System;
using System.Collections.Generic;
using System.Windows.Media;
using ProMik.Core.Interfaces.Settings;
using ProMik.SmartIct.Interfaces.Gui;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class ProMikSettingsData : ISettingsData
    {
        private const string PAGEKEYGENERAL = "pageKeyGeneral";
        private const string PAGENAMEGENERAL = "General";
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
        private const string USECONTAINSSETTINGKEY = "useContainsSettingKey";
        private const string USECONTAINSSETTINGNAME = "Use \"contains\" for component determination instead of \"start with\"";
        private const string FONTSIZESETTINGKEY = "fontSizeSettingKey";
        private const string FONTSIZESETTINGNAME = "Fontsize of names:";
        private const string SHOWNETNAMESSETTINGKEY = "showNetNamesSettingKey";
        private const string SHOWNETNAMESSETTINGNAME = "Show net names";
        private const string SHOWBUTTONSSETTINGKEY = "showButtonsSettingKey";
        private const string SHOWBUTTONSSETTINGNAME = "Show zoom and mirroring buttons";
        private const string PAGEKEYAPI = "pageKeyApi";
        private const string PAGENAMEAPI = "PCB Investigator API";
        private const string SECTIONAPIKEY = "sectionApiKey";
        private const string SECTIONAPINAME = PAGENAMEAPI;
        private const string STEPSSETTINGKEY = "stepsSettingKey";
        private const string STEPSSETTINGNAME = "Steps to be read by PCB Investigator API (a;b;c)(Beginning with 1, -1 = all):";
        private const string PAGEKEYCONNECTION = "pageKeyConnection";
        private const string PAGENAMECONNECTION = "GRPC Server API Connection";
        private const string CONNECTIONSECTIONKEY = "connectionSectionKey";
        private const string IPSETTINGKEY = "ipSettingKey";
        private const string IPSETTINGNAME = "IP address of GRPC API server:";
        private const string CONNECTIONSECTIONNAME = PAGENAMECONNECTION;
        private const string PAGEKEYTESTCOVERAGE = "pageKeyTestCoverage";
        private const string PAGENAMETESTCOVERAGE = "Test Coverage";
        private const string GNDSECTIONKEY = "gndSectionKey";
        private const string GNDSECTIONNAME = "GND nets settings";
        private const string GNDIDENTSETTINGKEY = "gndIdentSettingKey";
        private const string GNDIDENTSETTINGNAME = "GND nets identifier (a;b;c):";
        private const string GNDBLACKSETTINGKEY = "gndBlackSettingKey";
        private const string BLACKLISTNAME = "Blacklist of nets (a;b;c):";
        private const string POWERSECTIONKEY = "powerSectionKey";
        private const string POWERSECTIONNAME = "Power nets settings";
        private const string POWERIDENTSETTINGKEY = "POWERIdentSettingKey";
        private const string POWERIDENTSETTINGNAME = "Power nets identifier (a;b;c):";
        private const string POWERBLACKSETTINGKEY = "POWERBlackSettingKey";
        private const string JTAGSECTIONKEY = "JTAGSectionKey";
        private const string JTAGSECTIONNAME = "JTAG settings";
        private const string JTAGBLACKSETTINGKEY = "JTAGBlackSettingKey";
        private const string JTAGPINSNAME = "JTAG nets identifier (a;b;c)";
        private const string JTAGPINSKEY = "JTAGExtendedPinsSettingKey";
        private const string DEFJTAGPINSVALUE = ProMik.SmartIct.JtagPinInformationExtractor.Implementations.JJtagPinInformationCreator
            .DEFJTAGPINIDENTIFIER;

        private const string PAGEKEYSVF = "pageKeySvf";
        private const string PAGENAMESVF = "SVF Player";
        private const string SECTIONSVFKEY = "sectionSvfProgrammerKey";
        private const string SECTIONSVFNAME = "Programmer Connection";
        private const string SVFPGMSETTINGKEY = "svfPgmSettingKey";
        private const string SVFPGMNAME = "Programmer IP:";
        private const string SVFPORTSETTINGKEY = "svfPortSettingKey";
        private const string SVFPORTNAME = "Programmer Port:";
        private const string SVFSUPPLYVSETTINGKEY = "svfSupplyVoltageSettingKey";
        private const string SVFSUPPLYVNAME = "Supply Voltage in mv:";
        private const string SVFIOVOLTAGESETTINGKEY = "svfIoVoltageSettingKey";
        private const string SVFIOVOLTAGENAME = "IO Voltage in mv:";
        private const string SVFFREQUENCYKEY = "svfFrequencyKey";
        private const string SVFFREQUENCYNAME = "Default frequency to use in KHz";
        private const string SVFCABLECOMPKEY = "svfCableCompKey";
        private const string SVFCABLECOMPNAME = "Default cable compensation to use";
        private const string SVFTARGETSETTINGKEY = "svfTargetSettingKey";
        private const string SVFTARGETNAME = "Programmer Target";
        private const string SVFSLOTSETTINGKEY = "svfSlotSettingKey";
        private const string SVFSLOTNAME = "Programmer Slot";
        private const string ASKFORSVFSESETTINGSKEY = "svfUseSettingsKey";
        private const string ASKFORSESETTINGSNAME = "Ask the user for programmer related settings";
        private readonly ISettingsService settingsService;
        private readonly ILogger logger;
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
        private string jtagPinIdentifier;
        private string jtagNetBlacklist;
        private string powerNetIdentifier;
        private string powerNetBlacklist;
        private string gndNetIdentifier;
        private string gndNetBlacklist;
        private string pgmIp;
        private uint pgmPort;
        private uint supplyVoltageMv;
        private uint ioVoltageMv;
        private uint frequency;
        private uint cableCompensation;
        private bool askForSvfSettings;
        private TargetDef target;
        private SlotDef slot;
        private bool useContains;

        public ProMikSettingsData(ISettingsService settingsService, ILogger logger)
        {
            this.settingsService = settingsService;
            this.logger = logger;
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

        public bool UseContains
        {
            get
            {
                return useContains;
            }

            set
            {
            }
        }

        public TargetDef Target
        {
            get
            {
                return target;
            }

            set
            {
            }
        }

        public SlotDef Slot
        {
            get
            {
                return slot;
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

        public string JTAGPinIdentifier
        {
            get
            {
                return jtagPinIdentifier;
            }

            set
            {
            }
        }

        public uint Frequency
        {
            get
            {
                return frequency;
            }

            set
            {
            }
        }

        public uint CableCompensation
        {
            get
            {
                return cableCompensation;
            }

            set
            {
            }
        }

        public bool AskForSvfSettings
        {
            get
            {
                return askForSvfSettings;
            }

            set
            {
            }
        }

        public void InitContent()
        {
            settingsService.Reinitialize();
            resistorChars = GetListContent(GetSettingValue<string>(RCHARSSETTINGKEY));
            resistorColor = GetSettingValue<System.Drawing.Color>(RCOLORSETTINGKEY).ConvertToMediaColor();
            resistorNamesChecked = GetSettingValue<bool>(RCHECKNAMESETTINGKEY);
            capacitorChars = GetListContent(GetSettingValue<string>(CCHARSSETTINGKEY));
            capacitorColor = GetSettingValue<System.Drawing.Color>(CCOLORSETTINGKEY).ConvertToMediaColor();
            capacitorNamesChecked = GetSettingValue<bool>(CCHECKNAMESETTINGKEY);
            inductionChars = GetListContent(GetSettingValue<string>(ICHARSSETTINGKEY));
            inductionColor = GetSettingValue<System.Drawing.Color>(ICOLORSETTINGKEY).ConvertToMediaColor();
            inductionNamesChecked = GetSettingValue<bool>(ICHECKNAMESETTINGKEY);
            icChars = GetListContent(GetSettingValue<string>(ICCHARSSETTINGKEY));
            icColor = GetSettingValue<System.Drawing.Color>(ICCOLORSETTINGKEY).ConvertToMediaColor();
            icNamesChecked = GetSettingValue<bool>(ICCHECKNAMESETTINGKEY);
            connectorChars = GetListContent(GetSettingValue<string>(CONCHARSSETTINGKEY));
            connectorColor = GetSettingValue<System.Drawing.Color>(CONCOLORSETTINGKEY).ConvertToMediaColor();
            connectorNamesChecked = GetSettingValue<bool>(CONCHECKNAMESETTINGKEY);
            testpointChars = GetListContent(GetSettingValue<string>(TPCHARSSETTINGKEY));
            testpointColor = GetSettingValue<System.Drawing.Color>(TPCOLORSETTINGKEY).ConvertToMediaColor();
            testpointNamesChecked = GetSettingValue<bool>(TPCHECKNAMESETTINGKEY);
            compColor = GetSettingValue<System.Drawing.Color>(COMPCOLORSETTINGKEY).ConvertToMediaColor();
            compNamesChecked = GetSettingValue<bool>(COMPCHECKNAMESETTINGKEY);
            fontSizeNames = (int)GetSettingValue<long>(FONTSIZESETTINGKEY);
            netNamesChecked = GetSettingValue<bool>(SHOWNETNAMESSETTINGKEY);
            showButtons = GetSettingValue<bool>(SHOWBUTTONSSETTINGKEY);
            steps = GetSettingValue<string>(STEPSSETTINGKEY);
            ipAddress = GetSettingValue<string>(IPSETTINGKEY);
            jtagPinIdentifier = GetSettingValue<string>(JTAGPINSKEY);
            jtagNetBlacklist = GetSettingValue<string>(JTAGBLACKSETTINGKEY);
            powerNetIdentifier = GetSettingValue<string>(POWERIDENTSETTINGKEY);
            powerNetBlacklist = GetSettingValue<string>(POWERBLACKSETTINGKEY);
            gndNetIdentifier = GetSettingValue<string>(GNDIDENTSETTINGKEY);
            gndNetBlacklist = GetSettingValue<string>(GNDBLACKSETTINGKEY);
            pgmIp = GetSettingValue<string>(SVFPGMSETTINGKEY);
            pgmPort = (uint)GetSettingValue<long>(SVFPORTSETTINGKEY);
            supplyVoltageMv = (uint)GetSettingValue<long>(SVFSUPPLYVSETTINGKEY);
            ioVoltageMv = (uint)GetSettingValue<long>(SVFIOVOLTAGESETTINGKEY);
            frequency = (uint)GetSettingValue<long>(SVFFREQUENCYKEY);
            cableCompensation = (uint)GetSettingValue<long>(SVFCABLECOMPKEY);
            target = (TargetDef)GetSettingValue<TargetDef>(SVFTARGETSETTINGKEY);
            slot = (SlotDef)GetSettingValue<SlotDef>(SVFSLOTSETTINGKEY);
            askForSvfSettings = GetSettingValue<bool>(ASKFORSVFSESETTINGSKEY);
            useContains = GetSettingValue<bool>(USECONTAINSSETTINGKEY);
            CheckForNullvalue(ref steps);
            CheckForNullvalue(ref ipAddress);
            CheckForNullvalue(ref jtagNetBlacklist);
            CheckForNullvalue(ref powerNetIdentifier);
            CheckForNullvalue(ref powerNetBlacklist);
            CheckForNullvalue(ref gndNetIdentifier);
            CheckForNullvalue(ref gndNetBlacklist);
            CheckForNullvalue(ref pgmIp);
            CheckForNullvalue(ref jtagPinIdentifier);
        }

        public T GetSettingValue<T>(string key)
        {
            try
            {
                return settingsService.GetSettingValue<T>(key);
            }
            catch (Exception e)
            {
                logger.LogMessage("Setting for key not found: " + key + ": " + e.Message, LogCategory.WARNING);
                return (T)default;
            }
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

            IList<string> list = new List<string>(value.Split(";"));
            return list;
        }

        private static void DefineSvfSettings(IPage svfPage)
        {
            var section = svfPage.AddSection(SECTIONSVFKEY, SECTIONSVFNAME);
            section.AddStringSetting(SVFPGMSETTINGKEY, SVFPGMNAME, JsonSettingsStorageManager.PgmIpDef);
            section.AddLongSetting(SVFPORTSETTINGKEY, SVFPORTNAME, (int)JsonSettingsStorageManager.PgmPortDef);
            section.AddEnumSetting(SVFTARGETSETTINGKEY, SVFTARGETNAME, TargetDef.Channel_A);
            section.AddEnumSetting(SVFSLOTSETTINGKEY, SVFSLOTNAME, SlotDef.Slot_1);
            section.AddLongSetting(SVFSUPPLYVSETTINGKEY, SVFSUPPLYVNAME, (int)JsonSettingsStorageManager.SupplyVoltageMvDef);
            section.AddLongSetting(SVFIOVOLTAGESETTINGKEY, SVFIOVOLTAGENAME, (int)JsonSettingsStorageManager.IoVoltageMvDef);
            section.AddLongSetting(SVFFREQUENCYKEY, SVFFREQUENCYNAME, (int)JsonSettingsStorageManager.FrequencyDef);
            section.AddLongSetting(SVFCABLECOMPKEY, SVFCABLECOMPNAME, (int)JsonSettingsStorageManager.CableCompensationDef);
            section.AddBooleanSetting(
                ASKFORSVFSESETTINGSKEY,
                ASKFORSESETTINGSNAME,
                JsonSettingsStorageManager.AskForSvfSettingsDef);
        }

        private static void DefineTestCoverageSettings(IPage pageTestCoverage)
        {
            var gndSection = pageTestCoverage.AddSection(GNDSECTIONKEY, GNDSECTIONNAME);
            gndSection.AddStringSetting(GNDIDENTSETTINGKEY, GNDIDENTSETTINGNAME, JsonSettingsStorageManager.GndDef);
            gndSection.AddStringSetting(GNDBLACKSETTINGKEY, BLACKLISTNAME, string.Empty);
            var powerSection = pageTestCoverage.AddSection(POWERSECTIONKEY, POWERSECTIONNAME);
            powerSection.AddStringSetting(POWERIDENTSETTINGKEY, POWERIDENTSETTINGNAME, JsonSettingsStorageManager.PowerDef);
            powerSection.AddStringSetting(POWERBLACKSETTINGKEY, BLACKLISTNAME, string.Empty);
            var jtagSection = pageTestCoverage.AddSection(JTAGSECTIONKEY, JTAGSECTIONNAME);
            jtagSection.AddStringSetting(JTAGPINSKEY, JTAGPINSNAME, DEFJTAGPINSVALUE);
            jtagSection.AddStringSetting(JTAGBLACKSETTINGKEY, BLACKLISTNAME, string.Empty);
        }

        private static void DefineConnectionSettings(IPage pageConnection)
        {
            var section = pageConnection.AddSection(CONNECTIONSECTIONKEY, CONNECTIONSECTIONNAME);
            section.AddStringSetting(IPSETTINGKEY, IPSETTINGNAME, JsonSettingsStorageManager.Ipdef);
        }

        private static void DefineApiSettings(IPage pageApi)
        {
            var sectionApi = pageApi.AddSection(SECTIONAPIKEY, SECTIONAPINAME);
            sectionApi.AddStringSetting(STEPSSETTINGKEY, STEPSSETTINGNAME, JsonSettingsStorageManager.StepsDef);
        }

        private static void DefineGeneralSettings(IPage pageGeneral)
        {
            var sectionR = pageGeneral.AddSection(SECTIONRKEY, SECTIONRNAME);
            sectionR.AddStringSetting(RCHARSSETTINGKEY, RCHARSETTINGSNAME, JsonSettingsStorageManager.RDef);
            sectionR.AddColorSetting(RCOLORSETTINGKEY, RCOLORSETTINGSNAME, JsonSettingsStorageManager.RDefColor.ToString());
            sectionR.AddBooleanSetting(RCHECKNAMESETTINGKEY, SHOWNAMES, false);
            var sectionC = pageGeneral.AddSection(SECTIONCKEY, SECTIONCNAME);
            sectionC.AddStringSetting(CCHARSSETTINGKEY, CCHARSETTINGSNAME, JsonSettingsStorageManager.CDef);
            sectionC.AddColorSetting(CCOLORSETTINGKEY, CCOLORSETTINGSNAME, JsonSettingsStorageManager.CDefColor.ToString());
            sectionC.AddBooleanSetting(CCHECKNAMESETTINGKEY, SHOWNAMES, false);
            var sectionI = pageGeneral.AddSection(SECTIONIKEY, SECTIONINAME);
            sectionI.AddStringSetting(ICHARSSETTINGKEY, ICHARSETTINGSNAME, JsonSettingsStorageManager.IDef);
            sectionI.AddColorSetting(ICOLORSETTINGKEY, ICOLORSETTINGSNAME, JsonSettingsStorageManager.IDefColor.ToString());
            sectionI.AddBooleanSetting(ICHECKNAMESETTINGKEY, SHOWNAMES, false);
            var sectionIc = pageGeneral.AddSection(SECTIONICKEY, SECTIONICNAME);
            sectionIc.AddStringSetting(ICCHARSSETTINGKEY, ICCHARSETTINGSNAME, JsonSettingsStorageManager.IcDef);
            sectionIc.AddColorSetting(ICCOLORSETTINGKEY, ICCOLORSETTINGSNAME, JsonSettingsStorageManager.IcDefColor.ToString());
            sectionIc.AddBooleanSetting(ICCHECKNAMESETTINGKEY, SHOWNAMES, false);
            var sectionCon = pageGeneral.AddSection(SECTIONCONKEY, SECTIONCONNAME);
            sectionCon.AddStringSetting(CONCHARSSETTINGKEY, CONCHARSETTINGSNAME, JsonSettingsStorageManager.ConDef);
            sectionCon.AddColorSetting(
                CONCOLORSETTINGKEY,
                CONCOLORSETTINGSNAME,
                JsonSettingsStorageManager.ConDefColor.ToString());
            sectionCon.AddBooleanSetting(CONCHECKNAMESETTINGKEY, SHOWNAMES, true);
            var sectionTp = pageGeneral.AddSection(SECTIONTPKEY, SECTIONTPNAME);
            sectionTp.AddStringSetting(TPCHARSSETTINGKEY, TPCHARSETTINGSNAME, JsonSettingsStorageManager.TpDef);
            sectionTp.AddColorSetting(TPCOLORSETTINGKEY, TPCOLORSETTINGSNAME, JsonSettingsStorageManager.TpDefColor.ToString());
            sectionTp.AddBooleanSetting(TPCHECKNAMESETTINGKEY, SHOWNAMES, false);
            var sectionComp = pageGeneral.AddSection(SECTIONCOMPKEY, SECTIONCOMPNAME);
            sectionComp.AddColorSetting(
                COMPCOLORSETTINGKEY,
                COMPCOLORSETTINGSNAME,
                JsonSettingsStorageManager.CompDefColor.ToString());
            sectionComp.AddBooleanSetting(COMPCHECKNAMESETTINGKEY, SHOWNAMES, true);
            var sectionGen = pageGeneral.AddSection(SECTIONGENKEY, SECTIONGENNAME);
            sectionGen.AddBooleanSetting(USECONTAINSSETTINGKEY, USECONTAINSSETTINGNAME, false);
            sectionGen.AddLongSetting(FONTSIZESETTINGKEY, FONTSIZESETTINGNAME, JsonSettingsStorageManager.FontDef);
            sectionGen.AddBooleanSetting(SHOWNETNAMESSETTINGKEY, SHOWNETNAMESSETTINGNAME, false);
            sectionGen.AddBooleanSetting(SHOWBUTTONSSETTINGKEY, SHOWBUTTONSSETTINGNAME, true);
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
