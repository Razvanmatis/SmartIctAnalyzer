using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using GrpcClientParser.Interfaces;
using Interfaces.Gui;
using PinInformationExtractor.Interfaces;
using Prism.Commands;
using Prism.Regions;
using ProMik.Core.Interfaces.Events;
using SVFHelper.Interfaces;
using TestCoverage.Helper;
using TestCoverage.Implementations;
using TestCoverage.Interfaces;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Interfaces;

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
        private IProjectHandler projectHandlerToUse;
        private bool exportEnabled;
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

        public MenuViewModel(
            IRegionManager regionManager,
            IEventService eventService,
            ISettingsData settingsData,
            IBomDataModel bomData,
            IGrpcClientParserHandler grpcParser,
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
            ITestCoverageHandler testCoverageHandler)
            : base(regionManager)
        {
            this.bomHandler = bomHandler;
            this.projectLoadHandler = projectLoadHandler;
            this.logger = logger;
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

            ExportEnabled = false;
            ExitCommand = new DelegateCommand(() => ExitApplication());
            RunTestCoverageForDeterminingObjects = new DelegateCommand(() => testCoverageHandler.DetermineObjects(SetItemsObjects, GetActualItemsObject));
            GetCompleteTestCoverageOfJtags = new DelegateCommand(async () => await GetCompleteTestCoverageOfAllJtags().ConfigureAwait(false));
            GetTestCoverageForAllOthers = new DelegateCommand(async () => await TestCoverageForAllOthers().ConfigureAwait(false));
            RunTestCoverageForIcs = new DelegateCommand(async () => await TestCoverageForIcs().ConfigureAwait(false));
            RunTestCoverageForPullUpsDowns = new DelegateCommand(async () => await TestCoverageForPullUpsDowns().ConfigureAwait(false));
            RunTestCoverageForPullUps = new DelegateCommand(async () => await TestCoverageForPullUps().ConfigureAwait(false));
            RunTestCoverageForPullDowns = new DelegateCommand(async () => await TestCoverageForPullDowns().ConfigureAwait(false));
            CloseProjectFileCommand = new DelegateCommand(() => projectFileHandler.CloseProjectAction(ResetAll, ActionOfItems));
            AllSettingsCommand = new DelegateCommand(settingsHandler.OpenAllSettingsView);
            GetPinInformationJtags = new DelegateCommand(pinInformationExtractionHandler.GetPinInformationJtagsIntoFile);
            PlaySvfFile = new DelegateCommand(svfHandler.PlaySvfFileHandler);
            GenerateSvfFiles = new DelegateCommand(() => svfHandler.HandleSvfFileGeneration(projectHandlerToUse));
            OpenProjectFileCommand = new DelegateCommand(async () => await projectFileHandler.OpenProjectFile(manifestHandler.GetProjectSelectionPath(), ResetAll, ResetTestCoverage, ActionOfItems).ConfigureAwait(false));
            CreateProjectFileCommand = new DelegateCommand(() => projectFileHandler.CreateProjectFile(ResetAll, ActionOfItems));
            ExportSettingsCommand = new DelegateCommand(() => settingsHandler.HandleExportSettings(projectHandlerToUse));
            ImportSettingsCommand = new DelegateCommand(() => settingsHandler.HandleImportSettings(generalProjectHandler, true));
            ExportCommand = new DelegateCommand(async () => await projectLoadHandler.HandleExportJsonProject(projectHandlerToUse).ConfigureAwait(false));
            ImportCommand = new DelegateCommand(async () => await projectLoadHandler.HandleImportJsonProject(generalProjectHandler, true, ResetTestCoverage).ConfigureAwait(false));
            OpenCommand = new DelegateCommand(async () => await projectLoadHandler.OpenOdbFolder(generalProjectHandler, ResetTestCoverage).ConfigureAwait(false));
            ImportBomCommand = new DelegateCommand(() =>
            {
                bomData.ResetValues();
                bomHandler.OpenBomView();
            });
            eventService.Subscribe<HandleDropEvent>(async (x) => await dropHandler.HandleDropEventMethod(x, ResetAll, ResetTestCoverage, ActionOfItems).ConfigureAwait(false));
            eventService.Subscribe<ChangeExportMenuItemEnabledStateEvent>(HandleChangeExportMenuItemState);
            eventService.Subscribe<ComponentsImportFinishedEvent>(HandleComponentImportFinishedEvent);
            grpcParser.ChangeIpAdressOfClient(settingsData.IPAddress);
            ResetAllMenuItems();
        }

        public ICommand OpenProjectFileCommand { get; private set; }

        public ICommand CreateProjectFileCommand { get; private set; }

        public ICommand AllSettingsCommand { get; private set; }

        public ICommand CloseProjectFileCommand { get; private set; }

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

        public ICommand ImportBomCommand { get; private set; }

        public ICommand GetPinInformationJtags { get; private set; }

        public ICommand RunTestCoverageForPullUpsDowns { get; private set; }

        public ICommand RunTestCoverageForPullUps { get; private set; }

        public ICommand RunTestCoverageForPullDowns { get; private set; }

        public ICommand GenerateSvfFiles { get; private set; }

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

        private void ResetAll()
        {
            ResetTestCoverage();
            settingsData.UseValues = false;
            eventService.Publish<ResetViewEvent>(new ResetViewEvent());
            bomHandler.ResetBomView();
            bomData.ResetValues();
            manifestHandler.ResetValues();
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
            logger.LogMessage("All things are being resetted", LogCategory.INFO);
        }

        private void HandleChangeExportMenuItemState(ChangeExportMenuItemEnabledStateEvent obj)
        {
            ExportEnabled = obj.IsEnabled;
        }

        private void HandleComponentImportFinishedEvent(ComponentsImportFinishedEvent obj)
        {
            projectLoadHandler.HandleComponentImportFinishedEvent(obj, autoLoad, projectHandlerToUse);
        }

        private async Task TestCoverageForIcs()
        {
            SetItemsObjects(await testCoverageDeterminer.GetTestCoverage(GetActualItemsObject(), TestCoverageType.BOUNDARY_SCAN_ICS).ConfigureAwait(false));
        }

        private async Task TestCoverageForPullUps()
        {
            SetItemsObjects(await testCoverageDeterminer.GetTestCoverage(GetActualItemsObject(), TestCoverageType.BOUNDARY_SCAN_PULL_UPS).ConfigureAwait(false));
        }

        private async Task TestCoverageForPullDowns()
        {
            SetItemsObjects(await testCoverageDeterminer.GetTestCoverage(GetActualItemsObject(), TestCoverageType.BOUNDARY_SCAN_PULL_DOWNS).ConfigureAwait(false));
        }

        private async Task TestCoverageForPullUpsDowns()
        {
            SetItemsObjects(await testCoverageDeterminer.GetTestCoverage(GetActualItemsObject(), TestCoverageType.BOUNDARY_SCAN_PULL_UPS_DOWNS).ConfigureAwait(false));
        }

        private async Task TestCoverageForAllOthers()
        {
            SetItemsObjects(await testCoverageDeterminer.GetTestCoverage(GetActualItemsObject(), TestCoverageType.BOUNDARY_SCAN_OTHERS).ConfigureAwait(false));
        }

        private async Task GetCompleteTestCoverageOfAllJtags()
        {
            SetItemsObjects(await testCoverageDeterminer.GetTestCoverage(GetActualItemsObject(), TestCoverageType.BOUNDARY_SCAN_ALL).ConfigureAwait(false));
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
            eventService.Publish<SetCoverageValuesEvent>(new SetCoverageValuesEvent(items.TestCoverage, items.TestCoverageVisibilty));
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
