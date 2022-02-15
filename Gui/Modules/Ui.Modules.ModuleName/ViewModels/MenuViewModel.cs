using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using PinInformationExtractor.Interfaces;
using Prism.Commands;
using Prism.Regions;
using ProMik.Core.Interfaces.Events;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.PCBComponentParser.Interfaces;
using ProMik.SmartIct.Services.ReportCreator.Interfaces;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using ProMik.SmartIct.TestCoverageDeterminer.Helper;
using ProMik.SmartIct.TestCoverageDeterminer.Implementations;
using ProMik.SmartIct.TestCoverageDeterminer.Interfaces;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.Views;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class MenuViewModel : RegionViewModelBase
    {
        private readonly IEventService eventService;
        private readonly IBomDataModel bomData;
        private readonly ISettingsData settingsData;
        private readonly ITestCoverageDeterminer testCoverageDeterminer;
        private readonly ILogger logger;
        private readonly IManifestHandler manifestHandler;
        private readonly IBomHandler bomHandler;
        private readonly IProjectLoadHandler projectLoadHandler;
        private readonly ISvfHandler svfHandler;
        private IProjectHandler projectHandlerToUse;
        private ChartsView chartsView;
        private bool exportJsonAndBomEnabled;
        private string itemTestCoverageObjects;
        private string itemTestCoverageJtag;
        private string itemTestCoveragePullup;
        private string itemTestCoveragePulldown;
        private string itemTestCoveragePullupdown;
        private string itemTestCoverageOthers;
        private string itemTestCoverageAll;
        private bool testCoverageMenuItemsEnabled;
        private bool closeProjectEnabled;
        private bool autoLoad;
        private Visibility testCoverageVisibilty;
        private float testCoverage;
        private bool exportPdfEnabled;
        private IPinInformationExtractorHandler pinInformationExtractionHandler;
        private IPdfReportHandler reportPdfHandler;
        private ICsvReportCreator reportCsvHandler;

        public MenuViewModel(
            IRegionManager regionManager,
            IEventService eventService,
            ISettingsData settingsData,
            IBomDataModel bomData,
            IPCBComponentParser grpcParser,
            ITestCoverageDeterminer testCoverageDeterminer,
            ILogger logger,
            IDropHandler dropHandler,
            IManifestHandler manifestHandler,
            IProjectHandler generalProjectHandler,
            IProjectLoadHandler projectLoadHandler,
            ISettingsHandler settingsHandler,
            ISvfHandler svfHandler,
            IBomHandler bomHandler,
            IProjectFileHandler projectFileHandler,
            IPinInformationExtractorHandler pinInformationExtractionHandler,
            IPdfReportHandler reportPdfHandler,
            ITestCoverageHandler testCoverageHandler,
            ITestEventListener testEventListener,
            ICsvReportCreator reportCsvHandler)
            : base(regionManager)
        {
            this.bomHandler = bomHandler;
            this.pinInformationExtractionHandler = pinInformationExtractionHandler;
            this.svfHandler = svfHandler;
            this.projectLoadHandler = projectLoadHandler;
            this.logger = logger;
            this.reportCsvHandler = reportCsvHandler;
            this.reportPdfHandler = reportPdfHandler;
            testCoverageVisibilty = Visibility.Hidden;
            this.manifestHandler = manifestHandler;
            this.bomData = bomData;
            this.testCoverageDeterminer = testCoverageDeterminer;
            this.settingsData = settingsData;
            projectHandlerToUse = generalProjectHandler;
            this.eventService = eventService;
            void ActionOfItems(bool autoL, bool menuDis, IProjectHandler projectH)
            {
                autoLoad = autoL;
                CloseProjectEnabled = menuDis;
                projectHandlerToUse = projectH;
            }

            ExportJsonAndBomEnabled = false;
            ExitCommand = new DelegateCommand(() => ExitApplication());
            RunTestCoverageForDeterminingObjects = new DelegateCommand(
                () => testCoverageHandler.DetermineObjects(SetItemsObjects, GetActualItemsObject));
            GetCompleteTestCoverageOfJtags = new DelegateCommand(
                async () => await GetCompleteTestCoverageOfAllJtags().ConfigureAwait(false));
            GetTestCoverageForAllOthers = new DelegateCommand(
                async () => await TestCoverageForAllOthers().ConfigureAwait(false));
            RunTestCoverageForIcs = new DelegateCommand(
                async () => await TestCoverageForIcs().ConfigureAwait(false));
            RunTestCoverageForPullUpsDowns = new DelegateCommand(
                async () => await TestCoverageForPullUpsDowns().ConfigureAwait(false));
            RunTestCoverageForPullUps = new DelegateCommand(async () => await TestCoverageForPullUps().ConfigureAwait(false));
            RunTestCoverageForPullDowns = new DelegateCommand(async () => await TestCoverageForPullDowns().ConfigureAwait(false));
            CloseProjectFileCommand = new DelegateCommand(() => projectFileHandler.CloseProjectAction(ResetAll, ActionOfItems));
            AllSettingsCommand = new DelegateCommand(settingsHandler.OpenAllSettingsView);
            GetPinInformationJtags = new DelegateCommand(() => pinInformationExtractionHandler.GetPinInformationJtagsIntoFile());
            PlaySvfFile = new DelegateCommand(async () => await HandlerPlaySvfFile().ConfigureAwait(false));
            PlaySvfFolder = new DelegateCommand(async () => await HandlerPlaySvfFile(false).ConfigureAwait(false));
            GenerateSvfFiles = new DelegateCommand(async () => await HandleSfvFileCreation().ConfigureAwait(false));
            OpenProjectFileCommand = new DelegateCommand(
                async () => await projectFileHandler.OpenProjectFile(
                    manifestHandler.GetProjectSelectionPath(),
                    ResetAll,
                    ResetTestCoverage,
                    ActionOfItems).ConfigureAwait(false));
            CreateProjectFileCommand = new DelegateCommand(
                () => projectFileHandler.CreateProjectFileBase(ResetAll, ActionOfItems));
            SaveProjectFileCommand = new DelegateCommand(
                async () => await projectFileHandler.SaveProjectFile(projectHandlerToUse, ActionOfItems).ConfigureAwait(true));
            ExportSettingsCommand = new DelegateCommand(() => settingsHandler.HandleExportSettings(projectHandlerToUse));
            ImportSettingsCommand = new DelegateCommand(() => settingsHandler.HandleImportSettings(generalProjectHandler, true));
            ExportCommand = new DelegateCommand(
                async () => await projectLoadHandler.HandleExportJsonProject(projectHandlerToUse).ConfigureAwait(false));
            ImportCommand = new DelegateCommand(
                async () => await projectLoadHandler.HandleImportJsonProject(generalProjectHandler, true, ResetTestCoverage).ConfigureAwait(false));
            OpenCommand = new DelegateCommand(
                async () => await projectLoadHandler.OpenOdbFolder(generalProjectHandler, ResetTestCoverage)
                .ConfigureAwait(false));
            GetPdfFile = new DelegateCommand(() => reportPdfHandler.CreateReportFile());
            GetCsvFile = new DelegateCommand(() => reportCsvHandler.CreateReportFile());
            ImportBomCommand = new DelegateCommand(() =>
            {
                bomData.ResetValues();
                bomHandler.OpenBomView();
            });
            OpenChartWindow = new DelegateCommand(OpenTheChartWindow);
            eventService.Subscribe<HandleDropEvent>(
                async (x) => await dropHandler.HandleDropEventMethod(x, ResetAll, ResetTestCoverage, ActionOfItems)
                .ConfigureAwait(false));
            eventService.Subscribe<ChangeExportMenuItemEnabledStateEvent>(HandleChangeExportMenuItemState);
            eventService.Subscribe<ComponentsImportFinishedEvent>(HandleComponentImportFinishedEvent);
            eventService.Subscribe<SelectBomFinishEvent>(PerformAfterBomAction);
            grpcParser.ChangeIpAdressOfClient(settingsData.IPAddress);
            ResetAllMenuItems();
        }

        public ICommand OpenProjectFileCommand { get; private set; }

        public ICommand CreateProjectFileCommand { get; private set; }

        public ICommand SaveProjectFileCommand { get; }

        public ICommand AllSettingsCommand { get; private set; }

        public ICommand GetCsvFile { get; private set; }

        public ICommand CloseProjectFileCommand { get; private set; }

        public ICommand PlaySvfFolder { get; private set; }

        public ICommand OpenCommand { get; private set; }

        public ICommand ExitCommand { get; private set; }

        public ICommand PlaySvfFile { get; private set; }

        public ICommand GetCompleteTestCoverageOfJtags { get; private set; }

        public ICommand RunTestCoverageForIcs { get; private set; }

        public ICommand GetTestCoverageForAllOthers { get; private set; }

        public ICommand RunTestCoverageForDeterminingObjects { get; private set; }

        public ICommand ImportSettingsCommand { get; private set; }

        public ICommand ExportSettingsCommand { get; private set; }

        public ICommand ExportCommand { get; private set; }

        public ICommand ImportCommand { get; private set; }

        public ICommand OpenChartWindow { get; private set; }

        public ICommand ImportBomCommand { get; private set; }

        public ICommand GetPdfFile { get; private set; }

        public ICommand GetPinInformationJtags { get; private set; }

        public ICommand RunTestCoverageForPullUpsDowns { get; private set; }

        public ICommand RunTestCoverageForPullUps { get; private set; }

        public ICommand RunTestCoverageForPullDowns { get; private set; }

        public ICommand GenerateSvfFiles { get; private set; }

        public bool ExportPdfEnabled { get => exportPdfEnabled; set => SetProperty(ref exportPdfEnabled, value); }

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

        public bool CloseProjectEnabled
        {
            get
            {
                return closeProjectEnabled;
            }

            set
            {
                SetProperty(ref closeProjectEnabled, value);
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

        public bool ExportJsonAndBomEnabled
        {
            get
            {
                return exportJsonAndBomEnabled;
            }

            private set
            {
                SetProperty(ref exportJsonAndBomEnabled, value);
            }
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

        private void OpenTheChartWindow()
        {
            if (chartsView == null)
            {
                chartsView = new ChartsView();
                chartsView.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (chartsView.Height / 2);
                chartsView.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (chartsView.Width / 2);
                chartsView.Show();
            }
            else
            {
                if (!chartsView.IsVisible)
                {
                    ((ChartsViewModel)chartsView.DataContext).InitValues();
                    chartsView.Show();
                }
            }
        }

        private void ResetAll()
        {
            ResetTestCoverage();
            settingsData.UseValues = false;
            eventService.Publish<ResetViewEvent>(new ResetViewEvent());
            bomHandler.ResetBomView();
            bomData.ResetValues();
            manifestHandler.ResetValues();
            svfHandler.SavedSvfData.Clear();
        }

        private void ResetTestCoverage()
        {
            testCoverageDeterminer.ClearAllObjects();
            testCoverage = 0;
            testCoverageVisibilty = Visibility.Hidden;
            eventService.Publish<SetCoverageValuesEvent>(new SetCoverageValuesEvent(0, false));
            settingsData.UseValues = false;
            ResetAllMenuItems();
            TestCoverageMenuItemsEnabled = false;
            ExportPdfEnabled = false;
            logger.LogMessage("All things are being resetted", LogCategory.INFO);
        }

        private async Task HandleSfvFileCreation()
        {
            eventService.Publish(new SetBusyEvent(true));
            await Task.Run(() =>
            {
                svfHandler.HandleSvfFileGeneration(projectHandlerToUse, settingsData.JTAGPinIdentifier);
                ExportPdfEnabled = reportPdfHandler.ReportContent.Count > 0;
            }).ConfigureAwait(false);

            eventService.Publish(new SetBusyEvent(false));
        }

        private async Task HandlerPlaySvfFile(bool justFiles = true)
        {
            eventService.Publish(new SetBusyEvent(true));
            await Task.Run(() => svfHandler.PlaySvfFileHandler(projectHandlerToUse, justFiles)).ConfigureAwait(false);
            eventService.Publish(new SetBusyEvent(false));
        }

        private void PerformAfterBomAction(SelectBomFinishEvent obj)
        {
            bomHandler.PerformAfterBomAction(obj);
        }

        private void HandleChangeExportMenuItemState(ChangeExportMenuItemEnabledStateEvent obj)
        {
            ExportJsonAndBomEnabled = obj.IsEnabled;
        }

        private void HandleComponentImportFinishedEvent(ComponentsImportFinishedEvent obj)
        {
            projectLoadHandler.HandleComponentImportFinishedEvent(obj, autoLoad, projectHandlerToUse);
        }

        private async Task TestCoverageForIcs()
        {
            SetItemsObjects(await testCoverageDeterminer.GetTestCoverage(
                GetActualItemsObject(), TestCoverageType.BOUNDARY_SCAN_ICS).ConfigureAwait(false));
        }

        private async Task TestCoverageForPullUps()
        {
            SetItemsObjects(await testCoverageDeterminer.GetTestCoverage(
                GetActualItemsObject(), TestCoverageType.BOUNDARY_SCAN_PULL_UPS).ConfigureAwait(false));
        }

        private async Task TestCoverageForPullDowns()
        {
            SetItemsObjects(await testCoverageDeterminer.GetTestCoverage(
                GetActualItemsObject(), TestCoverageType.BOUNDARY_SCAN_PULL_DOWNS).ConfigureAwait(false));
        }

        private async Task TestCoverageForPullUpsDowns()
        {
            SetItemsObjects(await testCoverageDeterminer.GetTestCoverage(
                GetActualItemsObject(), TestCoverageType.BOUNDARY_SCAN_PULL_UPS_DOWNS).ConfigureAwait(false));
        }

        private async Task TestCoverageForAllOthers()
        {
            SetItemsObjects(await testCoverageDeterminer.GetTestCoverage(
                GetActualItemsObject(), TestCoverageType.BOUNDARY_SCAN_OTHERS).ConfigureAwait(false));
        }

        private async Task GetCompleteTestCoverageOfAllJtags()
        {
            SetItemsObjects(await testCoverageDeterminer.GetTestCoverage(
                GetActualItemsObject(), TestCoverageType.BOUNDARY_SCAN_ALL).ConfigureAwait(false));
        }

        private TestCoverageItems GetActualItemsObject()
        {
            return new TestCoverageItems(
                ItemTestCoverageObjects,
                ItemTestCoverageAll,
                ItemTestCoverageJtag,
                ItemTestCoverageOthers,
                ItemTestCoveragePulldown,
                ItemTestCoveragePullup,
                ItemTestCoveragePullupdown,
                testCoverageVisibilty == Visibility.Visible,
                testCoverage,
                TestCoverageMenuItemsEnabled);
        }

        private void SetItemsObjects(TestCoverageItems items)
        {
            ItemTestCoverageObjects = items.ItemTestCoverageObjects;
            ItemTestCoverageAll = items.ItemTestCoverageAll;
            ItemTestCoverageJtag = items.ItemTestCoverageJtag;
            ItemTestCoverageOthers = items.ItemTestCoverageOthers;
            ItemTestCoveragePulldown = items.ItemTestCoveragePulldown;
            ItemTestCoveragePullup = items.ItemTestCoveragePullup;
            ItemTestCoveragePullupdown = items.ItemTestCoveragePullupdown;
            testCoverageVisibilty = items.TestCoverageVisibilty ? Visibility.Visible : Visibility.Hidden;
            testCoverage = items.TestCoverage;
            eventService.Publish<SetCoverageValuesEvent>(
                new SetCoverageValuesEvent(items.TestCoverage, items.TestCoverageVisibilty));
            TestCoverageMenuItemsEnabled = items.TestCoverageMenuItemsEnabled;
        }

        private void ResetAllMenuItems()
        {
            ItemTestCoverageObjects = TestCoverageBoundaryScanDeterminer.ITEMNOTSELECTED;
            ItemTestCoverageAll = TestCoverageBoundaryScanDeterminer.ITEMNOTSELECTED;
            ItemTestCoverageJtag = TestCoverageBoundaryScanDeterminer.ITEMNOTSELECTED;
            ItemTestCoverageOthers = TestCoverageBoundaryScanDeterminer.ITEMNOTSELECTED;
            ItemTestCoveragePulldown = TestCoverageBoundaryScanDeterminer.ITEMNOTSELECTED;
            ItemTestCoveragePullup = TestCoverageBoundaryScanDeterminer.ITEMNOTSELECTED;
            ItemTestCoveragePullupdown = TestCoverageBoundaryScanDeterminer.ITEMNOTSELECTED;
        }

        private void ExitApplication()
        {
            manifestHandler.ResetValues();
            Environment.Exit(0);
        }
    }
}
