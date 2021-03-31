using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Grpc.Core;
using GrpcClientParser.Helper;
using GrpcClientParser.Interfaces;
using Interfaces.Gui;
using Interfaces.PcbInvestigator;
using PinInformationExtractor.Helper;
using PinInformationExtractor.Interfaces;
using Prism.Commands;
using Prism.Regions;
using ProMik.Core.Interfaces.Events;
using SVFHelper.Implementations;
using SVFHelper.Interfaces;
using TestCoverage.Helper;
using TestCoverage.Implementations;
using TestCoverage.Interfaces;
using Ui.Core.Mvvm;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Implementations;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.Views;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class MenuViewModel : RegionViewModelBase
    {
        private readonly ISettingsStorageManager settingsStorageManager;
        private readonly IEventService eventService;
        private readonly IBomDataModel bomData;
        private readonly ISettingsData settingsData;
        private readonly IGrpcClientParserHandler grpcParser;
        private readonly ITestCoverageDeterminer testCoverageDeterminer;
        private readonly IPinInformationExtractor pinInformationExtractor;
        private readonly ILogger logger;
        private readonly IManifestHandler manifestHandler;
        private readonly IProjectHandler generalProjectHandler;
        private readonly ISVFHelper svfHelper;
        private readonly IPackageService packageService;
        private readonly ISvfPlayer svfPlayer;
        private readonly IDialogSelector dialogSelector;
        private IProjectHandler projectHandlerToUse;
        private bool exportEnabled;
        private SelectBomView bomView;
        private string selectedPath;
        private IParsedResult result;
        private string itemTestCoverageObjects;
        private string itemTestCoverageJtag;
        private string itemTestCoveragePullup;
        private string itemTestCoveragePulldown;
        private string itemTestCoveragePullupdown;
        private string itemTestCoverageOthers;
        private string itemTestCoverageAll;
        private bool testCoverageMenuItemsEnabled;
        private IList<IPCBComponent> ics = new List<IPCBComponent>();
        private IList<IPCBComponent> pullUps = new List<IPCBComponent>();
        private IList<IPCBComponent> pullDowns = new List<IPCBComponent>();
        private bool closeProjectEnabled;
        private bool autoLoad;
        private Visibility testCoverageVisibilty;
        private float testCoverage;
        private AllSettingsView allSettingsView;

        public MenuViewModel(
            IRegionManager regionManager,
            IEventService eventService,
            ISettingsData settingsData,
            ISettingsStorageManager settingsStorageManager,
            IBomDataModel bomData,
            IGrpcClientParserHandler grpcParser,
            ITestCoverageDeterminer testCoverageDeterminer,
            IPinInformationExtractor pinInformationExtractor,
            ILogger logger,
            IManifestHandler manifestHandler,
            IProjectHandler generalProjectHandler,
            ISVFHelper svfHelper,
            IPackageService packageService,
            ISvfPlayer svfPlayer,
            IDialogSelector dialogSelector)
            : base(regionManager)
        {
            this.dialogSelector = dialogSelector;
            this.logger = logger;
            this.svfPlayer = svfPlayer;
            this.svfHelper = svfHelper;
            this.packageService = packageService;
            testCoverageVisibilty = Visibility.Hidden;
            this.generalProjectHandler = generalProjectHandler;
            this.manifestHandler = manifestHandler;
            this.pinInformationExtractor = pinInformationExtractor;
            this.bomData = bomData;
            this.testCoverageDeterminer = testCoverageDeterminer;
            this.settingsData = settingsData;
            projectHandlerToUse = generalProjectHandler;
            this.settingsStorageManager = settingsStorageManager;
            this.eventService = eventService;
            eventService.Subscribe<SelectBomFinishEvent>(PerformAfterBomAction);
            eventService.Subscribe<CloseAllSettingsEvent>(CloseAllSettingsView);
            eventService.Subscribe<HandleDropEvent>(async (x) => await HandleDropEventMethod(x).ConfigureAwait(false));
            ExitCommand = new DelegateCommand(() => ExitApplication());
            RunTestCoverageForDeterminingObjects = new DelegateCommand(RunTheTestCoverageForDeterminingObjects);
            this.grpcParser = grpcParser;
            GetCompleteTestCoverageOfJtags = new DelegateCommand(async () => await GetCompleteTestCoverageOfAllJtags().ConfigureAwait(false));
            ExportEnabled = false;
            GetTestCoverageForAllOthers = new DelegateCommand(async () => await TestCoverageForAllOthers().ConfigureAwait(false));
            RunTestCoverageForIcs = new DelegateCommand(async () => await TestCoverageForIcs().ConfigureAwait(false));
            RunTestCoverageForPullUpsDowns = new DelegateCommand(async () => await TestCoverageForPullUpsDowns().ConfigureAwait(false));
            eventService.Subscribe<ChangeExportMenuItemEnabledStateEvent>(HandleChangeExportMenuItemState);
            RunTestCoverageForPullUps = new DelegateCommand(async () => await TestCoverageForPullUps().ConfigureAwait(false));
            RunTestCoverageForPullDowns = new DelegateCommand(async () => await TestCoverageForPullDowns().ConfigureAwait(false));
            eventService.Subscribe<ComponentsImportFinishedEvent>(HandleComponentImportFinishedEvent);
            CloseProjectFileCommand = new DelegateCommand(CloseProjectAction);
            AllSettingsCommand = new DelegateCommand(OpenAllSettingsView);
            GetPinInformationJtags = new DelegateCommand(GetPinInformationJtagsIntoFile);
            PlaySvfFile = new DelegateCommand(PlaySvfFileHandler);
            GenerateSvfFiles = new DelegateCommand(HandleSvfFileGeneration);
            grpcParser.ChangeIpAdressOfClient(settingsData.IPAddress);
            OpenProjectFileCommand = new DelegateCommand(async () => await OpenProjectFile(manifestHandler.GetProjectSelectionPath()).ConfigureAwait(false));
            CreateProjectFileCommand = new DelegateCommand(CreateProjectFile);
            ImportBomCommand = new DelegateCommand(() =>
            {
                bomData.ResetValues();
                OpenBomView();
            });
            ExportSettingsCommand = new DelegateCommand(HandleExportSettings);
            ImportSettingsCommand = new DelegateCommand(() => HandleImportSettings(generalProjectHandler, true));
            ExportCommand = new DelegateCommand(async () => await HandleExportJsonProject().ConfigureAwait(false));
            ImportCommand = new DelegateCommand(async () => await HandleImportJsonProject(generalProjectHandler, true).ConfigureAwait(false));
            OpenCommand = new DelegateCommand(async () => await OpenOdbFolder(generalProjectHandler, true).ConfigureAwait(false));
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

        private async Task HandleDropEventMethod(HandleDropEvent obj)
        {
            string fileName = obj.FileName.ToLower(CultureInfo.CurrentCulture);
            if (fileName.EndsWith(".json", StringComparison.InvariantCulture) || fileName.EndsWith(".txt", StringComparison.InvariantCulture)
                || fileName.EndsWith(".db", StringComparison.InvariantCulture))
            {
                byte[] settingsContent = settingsStorageManager.IsValidSettingsFile(obj.FileName);
                if (settingsContent != null)
                {
                    HandleImportSettings(new DropDataSourceProvider(settingsContent, null), true);
                }
                else
                {
                    byte[] content = ((GeneralProjectHandler)generalProjectHandler).GetBytesOfFile(obj.FileName);
                    if (content != null)
                    {
                        await HandleImportJsonProject(new DropDataSourceProvider(null, content), true).ConfigureAwait(false);
                    }
                    else
                    {
                        logger.LogMessage("Not supported file dropped into application!", LogCategory.ERROR);
                    }
                }
            }
            else
            {
                await OpenProjectFile(obj.FileName).ConfigureAwait(false);
            }
        }

        private async Task OpenProjectFile(string path)
        {
            autoLoad = false;
            ResetAll();
            IProjectHandler handler = manifestHandler as IProjectHandler;
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            manifestHandler.SetPathAndManifestOfZipfile(path, false);
            if (!manifestHandler.IsManifestHandlingActive())
            {
                return;
            }

            CloseProjectEnabled = true;
            projectHandlerToUse = manifestHandler as IProjectHandler;
            byte[] settingsFile = manifestHandler.GetSettingsFile();
            if (settingsFile != null)
            {
                HandleImportSettings(handler, false);
            }

            byte[] jsonPathOfProject = manifestHandler.GetJsonProjectFile();
            if (jsonPathOfProject != null)
            {
                autoLoad = true;
                await HandleImportJsonProject(handler, false).ConfigureAwait(false);
            }
            else
            {
                string odbPath = await manifestHandler.GetOdbProjectPath().ConfigureAwait(true);
                if (!string.IsNullOrEmpty(odbPath))
                {
                    selectedPath = odbPath;
                    autoLoad = true;
                    await PerformLoadAction().ConfigureAwait(false);
                }
            }
        }

        private async Task PerformLoadAction()
        {
            try
            {
                ResetTestCoverage();
                settingsData.UseValues = false;
                eventService.Publish<SetBusyEvent>(new SetBusyEvent(true));
                eventService.Publish<ResetViewEvent>(new ResetViewEvent());
                bomView = null;
                AttributeList attributes = GeneralProjectHandler.GetAttributeList(settingsStorageManager.GetStorageContent());
                logger.LogMessage("Using of PCBInvestigator API for getting objects for path " + selectedPath + " started...", LogCategory.INFO);
                this.result = await grpcParser.GetParsedObjectsFromGrpcByZipFolder(selectedPath, settingsStorageManager.GetStorageContent().Steps, attributes.RDef, attributes.CDef, attributes.IDef, attributes.TDef, attributes.IcDef, attributes.ConDef).ConfigureAwait(true);
                var layerNames = GeneralProjectHandler.GetLayers(result);
                ((GeneralProjectHandler)generalProjectHandler).LogResults(layerNames, result);
                eventService.Publish<AddPcbObjectsEvent>(new AddPcbObjectsEvent(result.Components, result.Nets));
                eventService.Publish<SendLayersEvent>(new SendLayersEvent(layerNames));
            }
            catch (RpcException e)
            {
                logger.LogMessage("Error getting parsed objects: " + e.Message, LogCategory.ERROR);
                eventService.Publish<SetBusyEvent>(new SetBusyEvent(false));
            }
        }

        private async Task HandleImportJsonProject(IDataSourceProvider projectHandler, bool useSavingInProject)
        {
            byte[] data = projectHandler.GetJsonProjectContent();
            if (data != null)
            {
                ResetTestCoverage();
                eventService.Publish<SetBusyEvent>(new SetBusyEvent(true));
                eventService.Publish<ResetViewEvent>(new ResetViewEvent());
                AttributeList attributes = GeneralProjectHandler.GetAttributeList(settingsStorageManager.GetStorageContent());
                logger.LogMessage("Import of JSON data started for file...", LogCategory.INFO);
                await Task.Run(() =>
                {
                    this.result = grpcParser.ImportComponentsFromFile(
                    Encoding.ASCII.GetString(data),
                    attributes.RDef,
                    attributes.CDef,
                    attributes.IDef,
                    attributes.TDef,
                    attributes.IcDef,
                    attributes.ConDef);
                }).ConfigureAwait(true);
                if (this.result != null)
                {
                    if (useSavingInProject && manifestHandler.IsManifestHandlingActive())
                    {
                        if (manifestHandler.UpdateJsonProject(data))
                        {
                            logger.LogMessage("Successfully updated JSON project in project file", LogCategory.INFO);
                        }
                    }

                    settingsData.UseValues = CheckIfValuesAreBeingUsed();
                    var layerNames = GeneralProjectHandler.GetLayers(result);
                    ((GeneralProjectHandler)generalProjectHandler).LogResults(layerNames, result);
                    eventService.Publish<AddPcbObjectsEvent>(new AddPcbObjectsEvent(result.Components, result.Nets));
                    eventService.Publish<SendLayersEvent>(new SendLayersEvent(layerNames));
                }
                else
                {
                    eventService.Publish<SetBusyEvent>(new SetBusyEvent(false));
                }
            }
        }

        private void CreateProjectFile()
        {
            ResetAll();
            autoLoad = false;
            if (!dialogSelector.OpenGenericDialog(DialogType.SAVEFILE, TextRessources.SelectProjectFileToSave, "No valid target to save selected!", out string fileName))
            {
                return;
            }

            manifestHandler.SetPathAndManifestOfZipfile(fileName, true);
            projectHandlerToUse = manifestHandler as IProjectHandler;
            logger.LogMessage("Successfully created project file: " + fileName, LogCategory.INFO);
            CloseProjectEnabled = true;
        }

        private void ResetAll()
        {
            ResetTestCoverage();
            settingsData.UseValues = false;
            eventService.Publish<ResetViewEvent>(new ResetViewEvent());
            bomView = null;
            bomData.ResetValues();
            manifestHandler.ResetValues();
        }

        private void CloseProjectAction()
        {
            if (manifestHandler.IsManifestHandlingActive())
            {
                ResetAll();
                logger.LogMessage("Project file usage closed...", LogCategory.INFO);
            }
            else
            {
                logger.LogMessage("No project file usage was active!", LogCategory.WARNING);
            }

            projectHandlerToUse = generalProjectHandler;
            CloseProjectEnabled = false;
            autoLoad = false;
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

        private async Task HandleExportJsonProject()
        {
            if (this.result == null)
            {
                return;
            }

            string filePath = string.Empty;
            if (!dialogSelector.OpenGenericDialog(DialogType.SAVEFILE, TextRessources.ExportObjectData, "No valid target to save selected!", out filePath, TextRessources.JsonFilter))
            {
                return;
            }

            if (!filePath.ToLower(CultureInfo.CurrentCulture).EndsWith(".json", StringComparison.Ordinal))
            {
                filePath += ".json";
            }

            eventService.Publish<SetBusyEvent>(new SetBusyEvent(true));
            await Task.Run(() =>
            {
                byte[] data = Encoding.ASCII.GetBytes(grpcParser.GetDataAsString(this.result));
                projectHandlerToUse.ExportJsonProjectFileContent(data, filePath);
                eventService.Publish<SetBusyEvent>(new SetBusyEvent(false));
            }).ConfigureAwait(false);
        }

        private bool CheckAndPerformBomParsing()
        {
            if ((!string.IsNullOrEmpty(bomData.BomFile) || !string.IsNullOrEmpty(bomData.BomData)) && !string.IsNullOrEmpty(bomData.ColumnRef) && !string.IsNullOrEmpty(bomData.ColumnValue))
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

                if (string.IsNullOrEmpty(bomData.BomData))
                {
                    bomData.BomData = Encoding.ASCII.GetString(((GeneralProjectHandler)generalProjectHandler).GetBytesOfFile(bomData.BomFile));
                    if (string.IsNullOrEmpty(bomData.BomData))
                    {
                        return false;
                    }
                }

                CsvReader csvReader = new CsvReader(logger);
                var values = csvReader.ReadContentFromData(bomData.BomData, bomData.Separator, colRef, colValue);
                csvReader.SetValuesToObjectsFromCsv(result.Components, values);
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

            if (bomData.BomSettings == null)
            {
                bomData.SetBomSettings(new BomSettings(bomData.Separator, bomData.ColumnRef, bomData.ColumnValue));
            }

            if (!eventData.WasManuallyClosed)
            {
                bool result = CheckAndPerformBomParsing();
                settingsData.UseValues = result;
                if (!eventData.AutoModeEnabled && result && manifestHandler.IsManifestHandlingActive())
                {
                    bool newBom = manifestHandler.UpdateBomFile(Encoding.ASCII.GetBytes(bomData.BomData), bomData.BomFile[(bomData.BomFile.LastIndexOf("\\", StringComparison.Ordinal) + 1)..]);
                    if (newBom)
                    {
                        logger.LogMessage("Successfully saved BOM file in project file", LogCategory.INFO);
                        if (manifestHandler.UpdateBomSettingsFile(bomData.BomSettings, bomData.BomFile[(bomData.BomFile.LastIndexOf("\\", StringComparison.Ordinal) + 1)..] + "_settings"))
                        {
                            logger.LogMessage("Successfully saved BOM settings file in project file", LogCategory.INFO);
                        }
                    }
                }
            }
            else
            {
                settingsData.UseValues = false;
            }
        }

        private void OpenBomView(bool autoMode = false)
        {
            if (string.IsNullOrEmpty(bomData.BomData) || bomData.BomSettings == null)
            {
                bomView = new SelectBomView(eventService, false);
                bomView.Show();
            }
            else
            {
                eventService.Publish<SelectBomFinishEvent>(new SelectBomFinishEvent(false, autoMode));
            }
        }

        private async Task OpenOdbFolder(IProjectHandler projectHandler, bool useSavingIntoProject)
        {
            string path = await projectHandler.GetOdbProjectFolder().ConfigureAwait(true);
            if (!string.IsNullOrEmpty(path))
            {
                selectedPath = path;
                if (useSavingIntoProject && manifestHandler.IsManifestHandlingActive())
                {
                    await manifestHandler.UpdateOdbProject(path).ConfigureAwait(true);
                    logger.LogMessage("Successfully added ODB project files into project file", LogCategory.INFO);
                }

                await PerformLoadAction().ConfigureAwait(false);
            }
        }

        private void HandleExportSettings()
        {
            if (!dialogSelector.OpenGenericDialog(DialogType.SAVEFILE, TextRessources.ExportSettingsFile, "No valid target to save selected!", out string fileName, TextRessources.SettingsFilter))
            {
                return;
            }

            if (!fileName.ToLower(CultureInfo.CurrentCulture).EndsWith(".db", StringComparison.Ordinal))
            {
                fileName += ".db";
            }

            byte[] data = settingsStorageManager.GetSettingsContent();
            projectHandlerToUse.ExportSettingsFileContent(data, fileName);
        }

        private void HandleImportSettings(IDataSourceProvider projectHandler, bool useManifestSaving)
        {
            byte[] data = projectHandler.GetSettingsFileContent();
            if (data != null)
            {
                settingsStorageManager.ImportStorageContent(data);
                settingsData.InitContent();
                if (useManifestSaving && manifestHandler.IsManifestHandlingActive())
                {
                    manifestHandler.UpdateSettingsFile(data);
                    logger.LogMessage("Sucessfully added the settings file into the project file", LogCategory.INFO);
                }

                logger.LogMessage("Settings file was successfully imported", LogCategory.INFO);
            }
        }

        private void HandleComponentImportFinishedEvent(ComponentsImportFinishedEvent obj)
        {
            bool valuesAreBeingUsed = CheckIfValuesAreBeingUsed();
            if (!autoLoad && !valuesAreBeingUsed)
            {
                bomData.ResetValues();
                OpenBomView();
            }
            else if (!valuesAreBeingUsed)
            {
                bomData.ResetValues();
                BomSettings bomSettings = manifestHandler.GetBomSettingsFile();
                string bomFile = manifestHandler.GetBomFile();
                if (!string.IsNullOrEmpty(bomFile))
                {
                    bomData.BomData = projectHandlerToUse.GetBomData();
                }

                if (bomSettings != null)
                {
                    bomData.SetBomSettings(projectHandlerToUse.GetBomSettings());
                }

                if (bomSettings == null || string.IsNullOrEmpty(bomFile))
                {
                    OpenBomView();
                }
                else
                {
                    OpenBomView(true);
                }
            }
        }

        private void RunTheTestCoverageForDeterminingObjects()
        {
            if (result != null && result.Nets != null && result.Nets.Count > 0)
            {
                var content = settingsStorageManager.GetStorageContent();
                SetItemsObjects(testCoverageDeterminer.RunTheTestCoverageForDeterminingObjects(
                    result.Nets,
                    new IdentifierBlacklistContainer(
                        content.GndNetIdentifier,
                        content.GndNetBlacklist,
                        content.PowerNetIdentifier,
                        content.PowerNetBlacklist,
                        content.JTAGNetIdentifier,
                        content.JTAGNetBlacklist),
                    GetActualItemsObject(),
                    ref ics,
                    ref pullUps,
                    ref pullDowns));
            }
            else
            {
                logger.LogMessage("First load data into tool!", LogCategory.WARNING);
            }
        }

        private async Task TestCoverageForIcs()
        {
            SetItemsObjects(await testCoverageDeterminer.TestCoverageForIcs(GetActualItemsObject()).ConfigureAwait(false));
        }

        private async Task TestCoverageForPullUps()
        {
            SetItemsObjects(await testCoverageDeterminer.TestCoverageForPullUps(GetActualItemsObject()).ConfigureAwait(false));
        }

        private async Task TestCoverageForPullDowns()
        {
            SetItemsObjects(await testCoverageDeterminer.TestCoverageForPullDowns(GetActualItemsObject()).ConfigureAwait(false));
        }

        private async Task TestCoverageForPullUpsDowns()
        {
            SetItemsObjects(await testCoverageDeterminer.TestCoverageForPullUpsDowns(GetActualItemsObject()).ConfigureAwait(false));
        }

        private async Task TestCoverageForAllOthers()
        {
            SetItemsObjects(await testCoverageDeterminer.TestCoverageForAllOthers(GetActualItemsObject()).ConfigureAwait(false));
        }

        private async Task GetCompleteTestCoverageOfAllJtags()
        {
            SetItemsObjects(await testCoverageDeterminer.GetCompleteTestCoverageOfAllJtags(GetActualItemsObject()).ConfigureAwait(false));
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

        private void ExitApplication()
        {
            manifestHandler.ResetValues();
            Environment.Exit(0);
        }

        private void PlaySvfFileHandler()
        {
            if (!dialogSelector.OpenGenericDialog(DialogType.OPENFILE, TextRessources.SelectBsdlFile, "No valid BSDL file selected!", out string bsdlFile))
            {
                return;
            }

            var package = packageService.GetPackage(bsdlFile);
            if (package == null || package.BoundaryCells == null || package.BoundaryCells.Count == 0)
            {
                logger.LogMessage("Received invalid BSDL package out of BSDL file!", LogCategory.ERROR);
                return;
            }

            if (!dialogSelector.OpenGenericDialog(DialogType.OPENFILE, TextRessources.SelectVsfFile, "No valid SVF file selected", out string selectedSvf))
            {
                return;
            }

            if (!dialogSelector.OpenGenericDialog(DialogType.SAVEFILE, TextRessources.SvfLogPath, "No valid SVF log output path selected", out string logPath))
            {
                return;
            }

            var settingsContent = settingsStorageManager.GetStorageContent();
            if (svfPlayer.PlaySvfFile((uint)package.PinMap.Count, package.InstructionLength, selectedSvf, logPath, settingsContent.PgmIp, settingsContent.PgmPort, settingsContent.SupplyVoltageMv, settingsContent.IoVoltageMv))
            {
                logger.LogMessage("Successfully played the SVF file: " + selectedSvf, LogCategory.INFO);
            }
        }

        private void GetPinInformationJtagsIntoFile()
        {
            string fileName = string.Empty;
            if (!dialogSelector.OpenGenericDialog(DialogType.SAVEFILE, TextRessources.PinExportTitle, "No valid target to save selected!", out fileName))
            {
                return;
            }

            if (!fileName.ToLower(CultureInfo.CurrentCulture)[(fileName.Length - 4)..].Contains(".", StringComparison.Ordinal))
            {
                fileName += ".txt";
            }

            var result = pinInformationExtractor.CreatePinInformationFile(fileName, ics, pullUps, pullDowns);
            logger.LogMessage("PIN information exported into file " + fileName, LogCategory.INFO);
        }

        private void HandleSvfFileGeneration()
        {
            if (!dialogSelector.OpenGenericDialog(DialogType.OPENFOLDER, TextRessources.SelectSvfFolder, "No valid path selected", out string fileName))
            {
                return;
            }

            List<JtagConnectionInfo> result = pinInformationExtractor.GetAllPinInformation(ics, pullUps, pullDowns);
            if (result != null)
            {
                HandleBsdlFileCreation(result, fileName);
            }
        }

        private void HandleBsdlFileCreation(List<JtagConnectionInfo> result, string basePath)
        {
            foreach (var jtag in result)
            {
                if (!dialogSelector.OpenGenericDialog(DialogType.OPENFILE, "Select BSDL file for JTAG device: " + jtag.ComponentName, "No valid BSDL file selected!", out string bsdlFile))
                {
                    continue;
                }

                var package = packageService.GetPackage(bsdlFile);
                if (package == null || package.BoundaryCells == null || package.BoundaryCells.Count == 0)
                {
                    logger.LogMessage("Received invalid BSDL package out of BSDL file!", LogCategory.ERROR);
                    continue;
                }

                MessageBox.Show("Please make sure to have a programmer connected to the according JTAG device: " + jtag.ComponentName + " for being able to read out the default vector");
                var settingsContent = settingsStorageManager.GetStorageContent();
                byte[] defaultVector = svfPlayer.GetDefaultVector((uint)package.PinMap.Count, package.InstructionLength, settingsContent.PgmIp, settingsContent.PgmPort, settingsContent.SupplyVoltageMv, settingsContent.IoVoltageMv);
                if (defaultVector == null || defaultVector.Length == 0)
                {
                    logger.LogMessage("Were not able to get a valid default vector for target: " + jtag.ComponentName, LogCategory.ERROR);
                    continue;
                }

                int realLength = package.PinMap.Count / 8;
                if (package.PinMap.Count % 8 > 0)
                {
                    realLength++;
                }

                if (defaultVector.Length != realLength)
                {
                    logger.LogMessage("Created a default vector with invalid length! Length needs to be " + realLength + ", but it was " + defaultVector.Length, LogCategory.WARNING);
                }

                ISvfWriter writer = new PullDownSvfWriter(svfHelper);
                List<ISvfData> svfDataResult = new List<ISvfData>();
                svfDataResult.AddRange(writer.GetSVFFiles(basePath, jtag.ComponentName, pinInformationExtractor.GetAllGroundPins(jtag.Pins), package, defaultVector));
                writer = new PullUpSvfWriter(svfHelper);
                svfDataResult.AddRange(writer.GetSVFFiles(basePath, jtag.ComponentName, pinInformationExtractor.GetAllPowerPins(jtag.Pins), package, defaultVector));
                projectHandlerToUse.ExportSvfFiles(svfDataResult);
            }
        }

        private void CloseAllSettingsView(CloseAllSettingsEvent obj)
        {
            if (allSettingsView != null)
            {
                allSettingsView.Hide();
            }
        }

        private void OpenAllSettingsView()
        {
            if (allSettingsView == null)
            {
                allSettingsView = new AllSettingsView(eventService);
            }

            allSettingsView.Show();
        }

        private void ResetAllMenuItems()
        {
            ItemTestCoverageObjects = TestCoverageDeterminer.ITEMNOTSELECTED;
            ItemTestCoverageAll = TestCoverageDeterminer.ITEMNOTSELECTED;
            ItemTestCoverageJtag = TestCoverageDeterminer.ITEMNOTSELECTED;
            ItemTestCoverageOthers = TestCoverageDeterminer.ITEMNOTSELECTED;
            ItemTestCoveragePulldown = TestCoverageDeterminer.ITEMNOTSELECTED;
            ItemTestCoveragePullup = TestCoverageDeterminer.ITEMNOTSELECTED;
            ItemTestCoveragePullupdown = TestCoverageDeterminer.ITEMNOTSELECTED;
        }
    }
}
