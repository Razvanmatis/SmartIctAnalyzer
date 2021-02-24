using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Grpc.Core;
using GrpcClientParser;
using GrpcClientParser.Helper;
using GrpcClientParser.Interfaces;
using Interfaces;
using Interfaces.Gui;
using Interfaces.PCBApiObjects;
using Interfaces.PcbInvestigator;
using Interfaces.TestCoverage;
using Ookii.Dialogs.Wpf;
using PinInformationExtractor;
using Prism.Commands;
using Prism.Mvvm;
using ProMik.Services.Interfaces;
using ProMik.Services.Interfaces.Enums;
using TestCoverage;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.Views;
using Ui.Views;

namespace Ui.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private const string ITEMNOTSELECTED = "CheckboxBlankOutline";
        private const string ITEMSELECTED = "Checkbox";
        private static string[] testCoverageObjects = new string[] { "Pullups", "Pulldowns", "JTAG", "Others" };
        private IEventService eventService;
        private string title = "Smart ICT Tool";
        private GeneralSettingsView generalSettingsView;
        private bool isBusy;
        private ISettingsStorageManager settingsStorageManager;
        private IBomDataModel bomData;
        private SelectBomView bomView;
        private string selectedPath;
        private bool menuEnabled;
        private IGeneralSettingsData settingsData;
        private TestCoverageSettingsView testCoverageView;
        private IParsedResult result;
        private IGrpcClientParserHandler grpcParser;
        private bool exportEnabled;
        private ITestCoverageDeterminer testCoverageDeterminer;
        private float testCoverage;
        private Visibility testCoverageVisibility = Visibility.Hidden;
        private GrpcServerSettingsView grpcSettingsView;
        private string itemTestCoverageObjects;
        private string itemTestCoverageJtag;
        private string itemTestCoveragePullup;
        private string itemTestCoveragePulldown;
        private string itemTestCoveragePullupdown;
        private string itemTestCoverageOthers;
        private string itemTestCoverageAll;
        private bool testCoverageMenuItemsEnabled;
        private IPinInformationExtractor pinInformationExtractor;
        private IList<IPCBComponent> ics = new List<IPCBComponent>();
        private IList<IPCBComponent> pullUps = new List<IPCBComponent>();
        private IList<IPCBComponent> pullDowns = new List<IPCBComponent>();
        private ILogger logger;
        private int logIndex;

        public MainWindowViewModel(
            IEventService eventService,
            IGeneralSettingsData settingsData,
            ISettingsStorageManager settingsStorageManager,
            IBomDataModel bomData,
            IGrpcClientParserHandler grpcParser,
            ITestCoverageDeterminer testCoverageDeterminer,
            IPinInformationExtractor pinInformationExtractor,
            ILogger logger)
        {
            this.logger = logger;
            ResetAllMenuItems();
            this.pinInformationExtractor = pinInformationExtractor;
            this.bomData = bomData;
            this.testCoverageDeterminer = testCoverageDeterminer;
            this.settingsData = settingsData;
            this.settingsStorageManager = settingsStorageManager;
            AddDummyCommand = new DelegateCommand(async () => await AddDummyObject().ConfigureAwait(false));
            this.eventService = eventService;
            eventService.Subscribe<SelectBomFinishEvent>(PerformAfterBomAction);
            ExitCommand = new DelegateCommand(() => Environment.Exit(0));
            OpenCommand = new DelegateCommand(async () => await OpenOdbFolder().ConfigureAwait(false));
            eventService.Subscribe<SetBusyEvent>(HandleIsBusyEvent, ThreadOption.UIThread);
            GeneralSettingsCommand = new DelegateCommand(OpenGeneralSettingsView);
            TestCoverageSettingsCommand = new DelegateCommand(OpenTestCoverageSettingsView);
            eventService.Subscribe<CloseGeneralSettingsEvent>(CloseGeneralSettingsView);
            eventService.Subscribe<CloseTestCoverageSettingsEvent>(HandleCoseTestCoverageSettingsView);
            RunTestCoverageForDeterminingObjects = new DelegateCommand(RunTheTestCoverageForDeterminingObjects);
            ExportSettingsCommand = new DelegateCommand(HandleExportSettings);
            ImportSettingsCommand = new DelegateCommand(HandleImportSettings);
            MenuEnabled = true;
            ExportCommand = new DelegateCommand(async () => await HandleExportData().ConfigureAwait(false));
            ImportCommand = new DelegateCommand(async () => await HandleImport().ConfigureAwait(false));
            this.grpcParser = grpcParser;
            GetCompleteTestCoverageOfJtags = new DelegateCommand(async () => await GetCompleteTestCoverageOfAllJtags().ConfigureAwait(false));
            ExportEnabled = false;
            GetTestCoverageForAllOthers = new DelegateCommand(async () => await TestCoverageForAllOthers().ConfigureAwait(false));
            RunTestCoverageForIcs = new DelegateCommand(async () => await TestCoverageForIcs().ConfigureAwait(false));
            RunTestCoverageForPullUpsDowns = new DelegateCommand(async () => await TestCoverageForPullUpsDowns().ConfigureAwait(false));
            eventService.Subscribe<ChangeExportMenuItemEnabledStateEvent>(HandleChangeExportMenuItemState);
            ImportBomCommand = new DelegateCommand(OpenBomView);
            RunTestCoverageForPullUps = new DelegateCommand(async () => await TestCoverageForPullUps().ConfigureAwait(false));
            RunTestCoverageForPullDowns = new DelegateCommand(async () => await TestCoverageForPullDowns().ConfigureAwait(false));
            eventService.Subscribe<ComponentsImportFinishedEvent>(HandleComponentImportFinishedEvent);
            GrpcServerSettingsCommand = new DelegateCommand(OpenGrpcSettingsView);
            eventService.Subscribe<CloseGrpcSettingsEvent>(CloseGrpcSettingsView);
            GetPinInformationJtags = new DelegateCommand(GetPinInformationJtagsIntoFile);
            AddLogEntry = new DelegateCommand(AddLogEntryItem);
            grpcParser.ChangeIpAdressOfClient(settingsData.IPAddress);
        }

        public ICommand GrpcServerSettingsCommand { get; private set; }

        public ICommand AddDummyCommand { get; private set; }

        public ICommand OpenCommand { get; private set; }

        public ICommand ExitCommand { get; private set; }

        public ICommand GetCompleteTestCoverageOfJtags { get; private set; }

        public ICommand RunTestCoverageForIcs { get; private set; }

        public ICommand GetTestCoverageForAllOthers { get; private set; }

        public ICommand GeneralSettingsCommand { get; private set; }

        public ICommand TestCoverageSettingsCommand { get; private set; }

        public ICommand RunTestCoverageForDeterminingObjects { get; private set; }

        public ICommand ImportSettingsCommand { get; private set; }

        public ICommand ExportSettingsCommand { get; private set; }

        public ICommand ExportCommand { get; private set; }

        public ICommand ImportCommand { get; private set; }

        public ICommand ImportBomCommand { get; private set; }

        public ICommand GetPinInformationJtags { get; private set; }

        public ICommand RunTestCoverageForPullUpsDowns { get; private set; }

        public ICommand RunTestCoverageForPullUps { get; private set; }

        public ICommand RunTestCoverageForPullDowns { get; private set; }

        public ICommand AddLogEntry { get; private set; }

        public string ItemTestCoverageObjects
        {
            get
            {
                return itemTestCoverageObjects;
            }

            set
            {
                SetProperty(ref itemTestCoverageObjects, value);
            }
        }

        public string ItemTestCoverageJtag
        {
            get
            {
                return itemTestCoverageJtag;
            }

            set
            {
                SetProperty(ref itemTestCoverageJtag, value);
            }
        }

        public string ItemTestCoveragePullup
        {
            get
            {
                return itemTestCoveragePullup;
            }

            set
            {
                SetProperty(ref itemTestCoveragePullup, value);
            }
        }

        public string ItemTestCoveragePulldown
        {
            get
            {
                return itemTestCoveragePulldown;
            }

            set
            {
                SetProperty(ref itemTestCoveragePulldown, value);
            }
        }

        public string ItemTestCoveragePullupdown
        {
            get
            {
                return itemTestCoveragePullupdown;
            }

            set
            {
                SetProperty(ref itemTestCoveragePullupdown, value);
            }
        }

        public string ItemTestCoverageOthers
        {
            get
            {
                return itemTestCoverageOthers;
            }

            set
            {
                SetProperty(ref itemTestCoverageOthers, value);
            }
        }

        public string ItemTestCoverageAll
        {
            get
            {
                return itemTestCoverageAll;
            }

            set
            {
                SetProperty(ref itemTestCoverageAll, value);
            }
        }

        public float TestCoverage
        {
            get
            {
                return testCoverage;
            }

            set
            {
                SetProperty(ref testCoverage, value);
            }
        }

        public Visibility TestCoverageVisibilty
        {
            get
            {
                return testCoverageVisibility;
            }

            set
            {
                SetProperty(ref testCoverageVisibility, value);
            }
        }

        public bool ExportEnabled
        {
            get
            {
                return exportEnabled;
            }

            private set
            {
                SetProperty(ref exportEnabled, value);
            }
        }

        public bool MenuEnabled
        {
            get
            {
                return menuEnabled;
            }

            set
            {
                SetProperty(ref menuEnabled, value);
            }
        }

        public bool IsBusy
        {
            get
            {
                return isBusy;
            }

            set
            {
                SetProperty(ref isBusy, value);
                MenuEnabled = !value;
            }
        }

        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public bool TestCoverageMenuItemsEnabled
        {
            get
            {
                return testCoverageMenuItemsEnabled;
            }

            set
            {
                SetProperty(ref testCoverageMenuItemsEnabled, value);
            }
        }

        private static List<string> GetListFromString(string identifier)
        {
            List<string> list = new List<string>();
            foreach (var text in identifier.Split(";"))
            {
                list.Add(text);
            }

            return list;
        }

        private static List<string> GetLayers(IParsedResult result)
        {
            List<string> layerNames = new List<string>();
            foreach (var comp in result.Components)
            {
                if (!layerNames.Contains(comp.FunctionalAttributes.LayerName))
                {
                    layerNames.Add(comp.FunctionalAttributes.LayerName);
                }
            }

            return layerNames;
        }

        private static string ToggleItem(string value)
        {
            if (value.Equals(ITEMSELECTED))
            {
                return ITEMNOTSELECTED;
            }
            else
            {
                return ITEMSELECTED;
            }
        }

        private async Task AddDummyObject()
        {
            string pathToOdb = "C:\\Repositories\\smart_ict_analyser\\Testdaten\\panel";
            selectedPath = pathToOdb;
            await PerformLoadAction().ConfigureAwait(false);
        }

        private async Task PerformLoadAction()
        {
            try
            {
                ResetTestCoverage();
                settingsData.UseValues = false;
                IsBusy = true;
                eventService.Publish<ResetViewEvent>(new ResetViewEvent());
                bomView = null;
                ISettingsStorageContent content = settingsStorageManager.GetStorageContent();
                List<string> rDef = GetListFromString(content.RIdentifier);
                List<string> cDef = GetListFromString(content.CIdentifier);
                List<string> iDef = GetListFromString(content.IIdentifier);
                List<string> tDef = GetListFromString(content.TIdentifier);
                List<string> icDef = GetListFromString(content.ICIdentifier);
                List<string> conDef = GetListFromString(content.ConIdentifier);
                logger.LogMessage("Using of PCBInvestigator API for getting objects for path " + selectedPath + " started...", LogCategory.INFO);
                this.result = await grpcParser.GetParsedObjectsFromGrpcByZipFolder(selectedPath, rDef, cDef, iDef, tDef, icDef, conDef).ConfigureAwait(true);
                var layerNames = GetLayers(result);
                LogResults(layerNames, result);
                eventService.Publish<AddPcbObjectsEvent>(new AddPcbObjectsEvent(result.Components, result.Nets));
                eventService.Publish<SendLayersEvent>(new SendLayersEvent(layerNames));
            }
            catch (RpcException e)
            {
                logger.LogMessage("Error getting parsed objects: " + e.Message, LogCategory.ERROR);
                IsBusy = false;
            }
        }

        private void LogResults(List<string> layerNames, IParsedResult result)
        {
            string layers = string.Empty;
            foreach (var layer in layerNames)
            {
                layers += layer + ", ";
            }

            logger.LogMessage("Layers received: " + layers.Substring(0, layers.Length - 2), LogCategory.INFO);
            logger.LogMessage("Components received: " + result.Components.Count, LogCategory.INFO);
            logger.LogMessage("Nets received: " + result.Nets.Count, LogCategory.INFO);
            List<IPinComponent> pins = new List<IPinComponent>();
            foreach (var comp in result.Components)
            {
                foreach (var pin in comp.Connections)
                {
                    if (!pins.Contains(pin))
                    {
                        pins.Add(pin);
                    }
                }
            }

            logger.LogMessage("Pins received: " + pins.Count, LogCategory.INFO);
        }

        private async Task HandleImport()
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Title = TextResource.ImportDataFromFile;
            dialog.Filter = TextResource.JsonFilter;
            dialog.ShowDialog();
            string filePath = dialog.FileName;
            if (!string.IsNullOrEmpty(filePath))
            {
                ResetTestCoverage();
                IsBusy = true;
                eventService.Publish<ResetViewEvent>(new ResetViewEvent());
                ISettingsStorageContent content = settingsStorageManager.GetStorageContent();
                List<string> rDef = GetListFromString(content.RIdentifier);
                List<string> cDef = GetListFromString(content.CIdentifier);
                List<string> iDef = GetListFromString(content.IIdentifier);
                List<string> tDef = GetListFromString(content.TIdentifier);
                List<string> icDef = GetListFromString(content.ICIdentifier);
                List<string> conDef = GetListFromString(content.ConIdentifier);
                logger.LogMessage("Import of JSON data started for file " + filePath, LogCategory.INFO);
                await Task.Run(() =>
                {
                    this.result = grpcParser.ImportComponentsFromFile(
                    filePath,
                    rDef,
                    cDef,
                    iDef,
                    tDef,
                    icDef,
                    conDef);
                }).ConfigureAwait(true);
                if (this.result != null)
                {
                    settingsData.UseValues = CheckIfValuesAreBeingUsed();
                    var layerNames = GetLayers(result);
                    LogResults(layerNames, result);
                    eventService.Publish<AddPcbObjectsEvent>(new AddPcbObjectsEvent(result.Components, result.Nets));
                    eventService.Publish<SendLayersEvent>(new SendLayersEvent(layerNames));
                }
                else
                {
                    IsBusy = false;
                }
            }
        }

        private void ResetTestCoverage()
        {
            testCoverageDeterminer.ClearAllObjects();
            TestCoverageVisibilty = Visibility.Hidden;
            settingsData.UseValues = false;
            ResetAllMenuItems();
            TestCoverageMenuItemsEnabled = false;
            logger.LogMessage("All things are being resetted", LogCategory.INFO);
        }

        private void HandleChangeExportMenuItemState(ChangeExportMenuItemEnabledStateEvent obj)
        {
            ExportEnabled = obj.IsEnabled;
        }

        private bool CheckIfValuesAreBeingUsed()
        {
            foreach (var comp in result.Components)
            {
                if (!string.IsNullOrEmpty(comp.FunctionalAttributes.Value))
                {
                    return true;
                }
            }

            return false;
        }

        private async Task HandleExportData()
        {
            if (this.result == null)
            {
                return;
            }

            VistaSaveFileDialog dialog = new VistaSaveFileDialog();
            dialog.Title = TextResource.ExportObjectData;
            dialog.Filter = TextResource.JsonFilter;
            dialog.ShowDialog();
            string filePath = dialog.FileName;
            if (!string.IsNullOrEmpty(filePath))
            {
                if (!filePath.ToLower(CultureInfo.CurrentCulture).EndsWith(".json", StringComparison.Ordinal))
                {
                    filePath += ".json";
                }

                Application.Current.Dispatcher.Invoke(() =>
                {
                    logger.LogMessage("Export for data to JSON started into file " + filePath, LogCategory.INFO);
                });

                IsBusy = true;
                await Task.Run(() =>
                {
                    grpcParser.ExportComponentsToFile(this.result, filePath);
                    IsBusy = false;
                }).ConfigureAwait(false);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    logger.LogMessage("JSON export finished", LogCategory.INFO);
                });
            }
        }

        private void HandleIsBusyEvent(SetBusyEvent obj)
        {
            IsBusy = obj.IsBusy;
        }

        private bool CheckAndPerformBomParsing(IList<IPCBComponent> components)
        {
            if (!string.IsNullOrEmpty(bomData.BomFile) && !string.IsNullOrEmpty(bomData.ColumnRef) && !string.IsNullOrEmpty(bomData.ColumnValue))
            {
                if (!int.TryParse(bomData.ColumnRef, out int colRef))
                {
                    logger.LogMessage("Error parsing the column number for the REF column!", LogCategory.ERROR);
                    return false;
                }

                if (!int.TryParse(bomData.ColumnValue, out int colValue))
                {
                    logger.LogMessage("Error parsing the column number for the VALUE column!", LogCategory.ERROR);
                    return false;
                }

                CsvReader csvReader = new CsvReader(logger);
                var values = csvReader.ReadContentFromLine(bomData.BomFile, bomData.Separator, colRef, colValue);
                csvReader.SetValuesToObjectsFromCsv(components, values);
                eventService.Publish<UpdateBomDataEvent>(new UpdateBomDataEvent());
                logger.LogMessage("Import of BOM file " + bomData.BomFile + " finished", LogCategory.INFO);
                return true;
            }
            else
            {
                return false;
            }
        }

        private void PerformAfterBomAction(SelectBomFinishEvent eventData)
        {
            if (bomView != null && (eventData == null || !eventData.WasManuallyClosed))
            {
                bomView.Visibility = Visibility.Hidden;
            }

            if (!eventData.WasManuallyClosed)
            {
                settingsData.UseValues = CheckAndPerformBomParsing(result.Components);
            }
            else
            {
                settingsData.UseValues = false;
            }
        }

        private void OpenBomView()
        {
            bomData.ResetValues();
            bomView = new SelectBomView(eventService);
            bomView.Show();
        }

        private async Task OpenOdbFolder()
        {
            VistaFolderBrowserDialog dialog = new VistaFolderBrowserDialog();
            dialog.Description = TextResource.SelectTheOdbProjectPath;
            dialog.ShowNewFolderButton = false;
            dialog.ShowDialog();
            if (dialog.SelectedPath != null && !string.IsNullOrEmpty(dialog.SelectedPath))
            {
                selectedPath = dialog.SelectedPath;
                await PerformLoadAction().ConfigureAwait(false);
            }
        }

        private void OpenGeneralSettingsView()
        {
            if (generalSettingsView == null)
            {
                generalSettingsView = new GeneralSettingsView(eventService);
                generalSettingsView.Show();
            }
        }

        private void HandleExportSettings()
        {
            VistaSaveFileDialog saveDialog = new VistaSaveFileDialog();
            saveDialog.Title = TextResource.ExportSettingsFile;
            saveDialog.Filter = TextResource.SettingsFilter;
            saveDialog.ShowDialog();
            string fileName = saveDialog.FileName;
            if (!string.IsNullOrEmpty(fileName))
            {
                if (!fileName.ToLower(CultureInfo.CurrentCulture).EndsWith(".settings", StringComparison.Ordinal))
                {
                    fileName += ".settings";
                }

                settingsStorageManager.ExportStorageContent(fileName);
                logger.LogMessage("Settings file was exported to " + fileName, LogCategory.INFO);
            }
        }

        private void HandleImportSettings()
        {
            VistaOpenFileDialog dialog = new VistaOpenFileDialog();
            dialog.Title = TextResource.ImportSettingsFile;
            dialog.Filter = TextResource.SettingsFilter;
            dialog.ShowDialog();
            string path = dialog.FileName;
            if (!string.IsNullOrEmpty(path))
            {
                settingsStorageManager.ImportStorageContent(path);
                settingsData.InitContent();
                logger.LogMessage("Settings file was imported from " + path, LogCategory.INFO);
            }
        }

        private void HandleComponentImportFinishedEvent(ComponentsImportFinishedEvent obj)
        {
            if (!CheckIfValuesAreBeingUsed())
            {
                OpenBomView();
            }
        }

        private void OpenTestCoverageSettingsView()
        {
            if (testCoverageView == null)
            {
                testCoverageView = new TestCoverageSettingsView(eventService);
                testCoverageView.Show();
            }
        }

        private void CloseGeneralSettingsView(CloseGeneralSettingsEvent obj)
        {
            if (!obj.WasClosedManually)
            {
                generalSettingsView.Close();
            }

            generalSettingsView = null;
        }

        private void HandleCoseTestCoverageSettingsView(CloseTestCoverageSettingsEvent obj)
        {
            if (!obj.WasClosedManually)
            {
                testCoverageView.Close();
            }

            testCoverageView = null;
        }

        private void RunTheTestCoverageForDeterminingObjects()
        {
            if (result != null && result.Nets != null && result.Nets.Count > 0)
            {
                ISettingsStorageContent content = settingsStorageManager.GetStorageContent();
                testCoverageDeterminer.DefineGndNets(result.Nets, content.GndNetIdentifier, content.GndNetBlacklist);
                testCoverageDeterminer.DefinePowerNets(result.Nets, content.PowerNetIdentifier, content.PowerNetBlacklist);
                testCoverageDeterminer.DefineJtagNets(result.Nets, content.JTAGNetIdentifier, content.JTAGNetBlacklist);
                Dictionary<TestCoverageObject, IList<IPCBComponent>> mapObjects = new Dictionary<TestCoverageObject, IList<IPCBComponent>>();
                ics = testCoverageDeterminer.DefineIcs();
                pullUps = testCoverageDeterminer.DefinePullUpResistors();
                pullDowns = testCoverageDeterminer.DefinePullDownResistors();
                mapObjects.Add(TestCoverageObject.PULLUP, pullUps);
                mapObjects.Add(TestCoverageObject.PULLDOWN, pullDowns);
                mapObjects.Add(TestCoverageObject.JTAG, ics);
                mapObjects.Add(TestCoverageObject.OTHERS, testCoverageDeterminer.DefineOthers());
                LogTestCoverageObjects(mapObjects);
                eventService.Publish<AddTestCoverageObjectsEvent>(new AddTestCoverageObjectsEvent(mapObjects));
                eventService.Publish<TestCoverageForDeterminingObjectsPerformedEvent>(new TestCoverageForDeterminingObjectsPerformedEvent(testCoverageObjects.ToList()));
                ItemTestCoverageObjects = ITEMSELECTED;
                if (mapObjects.Count > 0)
                {
                    TestCoverageMenuItemsEnabled = true;
                }
            }
            else
            {
                ItemTestCoverageObjects = ITEMNOTSELECTED;
            }
        }

        private void LogTestCoverageObjects(Dictionary<TestCoverageObject, IList<IPCBComponent>> mapObjects)
        {
            logger.LogMessage("Determined pull up objects: " + mapObjects[TestCoverageObject.PULLUP].Count, LogCategory.INFO);
            logger.LogMessage("Determined pull down objects: " + mapObjects[TestCoverageObject.PULLDOWN].Count, LogCategory.INFO);
            logger.LogMessage("Determined IC objects: " + mapObjects[TestCoverageObject.JTAG].Count, LogCategory.INFO);
            logger.LogMessage("Determined other objects which are connected to the ICs: " + mapObjects[TestCoverageObject.OTHERS].Count, LogCategory.INFO);
        }

        private async Task TestCoverageForIcs()
        {
            bool value = false;
            IList<ITestCoverageResult> result = testCoverageDeterminer.DefineTestCoverageForIcObjects();
            if (result.Count > 0)
            {
                eventService.Publish<RefreshTestcoverageResultObjects>(new RefreshTestcoverageResultObjects());
                ItemTestCoverageJtag = ToggleItem(ItemTestCoverageJtag);
                value = !testCoverageDeterminer.GetTestsPerformedState(TestCoverageObject.JTAG);
            }
            else
            {
                logger.LogMessage("No ICs were detected before", LogCategory.WARNING);
                ItemTestCoverageJtag = ITEMNOTSELECTED;
            }

            testCoverageDeterminer.SetTestsPerformedState(TestCoverageObject.JTAG, value);
            DefineTestCoverageLabel(await testCoverageDeterminer.GetTestCoveragePercentageValueForIcs().ConfigureAwait(false));
        }

        private async Task TestCoverageForPullUps()
        {
            bool value = false;
            IList<ITestCoverageResult> results = await testCoverageDeterminer.DefineTestCoverageForPullUpDownObjects(testCoverageDeterminer.DefineIcs(), testCoverageDeterminer.DefinePullUpResistors(), false).ConfigureAwait(true);
            if (results.Count > 0)
            {
                eventService.Publish<RefreshTestcoverageResultObjects>(new RefreshTestcoverageResultObjects());
                ItemTestCoveragePullup = ToggleItem(ItemTestCoveragePullup);
                value = !testCoverageDeterminer.GetTestsPerformedState(TestCoverageObject.PULLUP);
            }
            else
            {
                logger.LogMessage("No pull ups were detected before", LogCategory.WARNING);
                ItemTestCoveragePullup = ITEMNOTSELECTED;
            }

            testCoverageDeterminer.SetTestsPerformedState(TestCoverageObject.PULLUP, value);
            DefineTestCoverageLabel(await testCoverageDeterminer.GetTestCoveragePercentageValue(testCoverageDeterminer.DefinePullUpResistors()).ConfigureAwait(false));
        }

        private async Task TestCoverageForPullDowns()
        {
            bool value = false;
            IList<ITestCoverageResult> results = await testCoverageDeterminer.DefineTestCoverageForPullUpDownObjects(testCoverageDeterminer.DefineIcs(), testCoverageDeterminer.DefinePullDownResistors()).ConfigureAwait(true);
            if (results.Count > 0)
            {
                eventService.Publish<RefreshTestcoverageResultObjects>(new RefreshTestcoverageResultObjects());
                ItemTestCoveragePulldown = ToggleItem(ItemTestCoveragePulldown);
                value = !testCoverageDeterminer.GetTestsPerformedState(TestCoverageObject.PULLDOWN);
            }
            else
            {
                logger.LogMessage("No pull downs were detected before", LogCategory.WARNING);
                ItemTestCoveragePulldown = ITEMNOTSELECTED;
            }

            testCoverageDeterminer.SetTestsPerformedState(TestCoverageObject.PULLDOWN, value);
            DefineTestCoverageLabel(await testCoverageDeterminer.GetTestCoveragePercentageValue(testCoverageDeterminer.DefinePullDownResistors()).ConfigureAwait(false));
        }

        private async Task TestCoverageForPullUpsDowns()
        {
            bool valueUp = false;
            bool valueDown = false;
            IList<ITestCoverageResult> results = await testCoverageDeterminer.DefineTestCoverageForPullUpDownObjects().ConfigureAwait(true);
            if (results.Count > 0)
            {
                eventService.Publish<RefreshTestcoverageResultObjects>(new RefreshTestcoverageResultObjects());
                ItemTestCoveragePullupdown = ToggleItem(ItemTestCoveragePullupdown);
                if (ItemTestCoveragePullupdown.Equals(ITEMSELECTED))
                {
                    ItemTestCoveragePulldown = ITEMSELECTED;
                    ItemTestCoveragePullup = ITEMSELECTED;
                    valueUp = true;
                    valueDown = true;
                }
                else
                {
                    ItemTestCoveragePulldown = ITEMNOTSELECTED;
                    ItemTestCoveragePullup = ITEMNOTSELECTED;
                }
            }
            else
            {
                logger.LogMessage("No pull ups and pull downs were detected before", LogCategory.WARNING);
                ItemTestCoveragePulldown = ITEMNOTSELECTED;
                ItemTestCoveragePullup = ITEMNOTSELECTED;
                ItemTestCoveragePullupdown = ITEMNOTSELECTED;
            }

            testCoverageDeterminer.SetTestsPerformedState(TestCoverageObject.PULLUP, valueUp);
            testCoverageDeterminer.SetTestsPerformedState(TestCoverageObject.PULLDOWN, valueDown);
            DefineTestCoverageLabel(await testCoverageDeterminer.GetTestCoveragePercentageValue().ConfigureAwait(false));
        }

        private async Task TestCoverageForAllOthers()
        {
            bool valueBool = false;
            float value = await testCoverageDeterminer.GetTestCoveragePercentageValueForAllOtherObjects().ConfigureAwait(true);
            DefineTestCoverageLabel(value);
            if (value > 0)
            {
                ItemTestCoverageOthers = ToggleItem(ItemTestCoverageOthers);
                valueBool = !testCoverageDeterminer.GetTestsPerformedState(TestCoverageObject.OTHERS);
            }
            else
            {
                logger.LogMessage("No other components connected to ICs were detected before", LogCategory.WARNING);
                ItemTestCoverageOthers = ITEMNOTSELECTED;
            }

            testCoverageDeterminer.SetTestsPerformedState(TestCoverageObject.OTHERS, valueBool);
        }

        private async Task GetCompleteTestCoverageOfAllJtags()
        {
            bool valueUp = false;
            bool valueDown = false;
            bool valueJtag = false;
            bool someThingChanged = false;
            bool valueOthers = false;
            IList<ITestCoverageResult> listPullUpsPullDowns = await testCoverageDeterminer.DefineTestCoverageForPullUpDownObjects().ConfigureAwait(true);
            if (listPullUpsPullDowns != null && listPullUpsPullDowns.Count > 0)
            {
                someThingChanged = true;
            }

            IList<ITestCoverageResult> listJtags = testCoverageDeterminer.DefineTestCoverageForIcObjects();
            if (listJtags != null && listJtags.Count > 0)
            {
                someThingChanged = true;
            }

            float valueOfOthers = await testCoverageDeterminer.GetTestCoveragePercentageValueForAllOtherObjects().ConfigureAwait(true);
            if (valueOfOthers > 0)
            {
                someThingChanged = true;
            }

            DefineTestCoverageLabel(await testCoverageDeterminer.GetTestCoveragePercentageValue().ConfigureAwait(true) + valueOfOthers);
            string textValue = ITEMNOTSELECTED;
            if (someThingChanged)
            {
                eventService.Publish<RefreshTestcoverageResultObjects>(new RefreshTestcoverageResultObjects());
                ItemTestCoverageAll = ToggleItem(ItemTestCoverageAll);
                if (ItemTestCoverageAll.Equals(ITEMSELECTED))
                {
                    textValue = ITEMSELECTED;
                    valueUp = true;
                    valueDown = true;
                    valueJtag = true;
                    valueOthers = true;
                }
            }

            ItemTestCoveragePulldown = textValue;
            ItemTestCoveragePullup = textValue;
            ItemTestCoveragePullupdown = textValue;
            ItemTestCoverageJtag = textValue;
            ItemTestCoverageAll = textValue;
            if (someThingChanged)
            {
                ItemTestCoverageOthers = ITEMSELECTED;
            }
            else
            {
                ItemTestCoverageOthers = ITEMNOTSELECTED;
            }

            testCoverageDeterminer.SetTestsPerformedState(TestCoverageObject.PULLUP, valueUp);
            testCoverageDeterminer.SetTestsPerformedState(TestCoverageObject.PULLDOWN, valueDown);
            testCoverageDeterminer.SetTestsPerformedState(TestCoverageObject.JTAG, valueJtag);
            testCoverageDeterminer.SetTestsPerformedState(TestCoverageObject.OTHERS, valueOthers);
        }

        private void AddLogEntryItem()
        {
            if (++logIndex % 3 == 0)
            {
                logger.LogMessage("This is a test info message " + logIndex, LogCategory.INFO);
            }
            else if (logIndex % 3 == 1)
            {
                logger.LogMessage("This is a test warning message " + logIndex, LogCategory.WARNING);
            }
            else
            {
                logger.LogMessage("This is a test error message " + logIndex, LogCategory.ERROR);
            }
        }

        private void GetPinInformationJtagsIntoFile()
        {
            VistaSaveFileDialog saveDialog = new VistaSaveFileDialog();
            saveDialog.Title = TextResource.PinExportTitle;
            saveDialog.ShowDialog();
            string fileName = saveDialog.FileName;
            if (!string.IsNullOrEmpty(fileName))
            {
                if (!fileName.ToLower(CultureInfo.CurrentCulture).Substring(fileName.Length - 4).Contains(".", StringComparison.Ordinal))
                {
                    fileName += ".txt";
                }

                pinInformationExtractor.CreatePinInformationFile(fileName, ics, pullUps, pullDowns);
                logger.LogMessage("PIN information exported into file " + fileName, LogCategory.INFO);
            }
        }

        private void DefineTestCoverageLabel(float value)
        {
            TestCoverageVisibilty = Visibility.Visible;
            TestCoverage = value;
        }

        private void CloseGrpcSettingsView(CloseGrpcSettingsEvent obj)
        {
            if (grpcSettingsView != null)
            {
                grpcSettingsView.Hide();
                grpcSettingsView = null;
            }
        }

        private void OpenGrpcSettingsView()
        {
            if (grpcSettingsView == null)
            {
                grpcSettingsView = new GrpcServerSettingsView(eventService);
                grpcSettingsView.Show();
            }
        }

        private void ResetAllMenuItems()
        {
            ItemTestCoverageObjects = ITEMNOTSELECTED;
            ItemTestCoverageAll = ITEMNOTSELECTED;
            ItemTestCoverageJtag = ITEMNOTSELECTED;
            ItemTestCoverageOthers = ITEMNOTSELECTED;
            ItemTestCoveragePulldown = ITEMNOTSELECTED;
            ItemTestCoveragePullup = ITEMNOTSELECTED;
            ItemTestCoveragePullupdown = ITEMNOTSELECTED;
        }
    }
}
