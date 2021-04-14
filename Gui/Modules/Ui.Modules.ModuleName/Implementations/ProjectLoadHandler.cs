using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcClientParser.Interfaces;
using Interfaces.Gui;
using ProMik.Core.Interfaces.Events;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class ProjectLoadHandler : IProjectLoadHandler
    {
        private readonly IEventService eventService;
        private readonly ILogger logger;
        private readonly ISettingsStorageManager settingsStorageManager;
        private readonly IGrpcClientParserHandler grpcParser;
        private readonly IManifestHandler manifestHandler;
        private readonly ISettingsData settingsData;
        private readonly IResultModel resultModel;
        private readonly IDialogSelector dialogSelector;
        private readonly IBomDataModel bomData;
        private readonly IBomHandler bomHandler;
        private readonly IProjectHandler generalProjectHandler;

        public ProjectLoadHandler(
            IEventService eventService,
            ILogger logger,
            ISettingsStorageManager settingsStorageManager,
            IGrpcClientParserHandler grpcParser,
            IManifestHandler manifestHandler,
            ISettingsData settingsData,
            IResultModel resultModel,
            IDialogSelector dialogSelector,
            IBomDataModel bomData,
            IBomHandler bomHandler,
            IProjectHandler generalProjectHandler)
        {
            this.eventService = eventService;
            this.logger = logger;
            this.bomData = bomData;
            this.bomHandler = bomHandler;
            this.generalProjectHandler = generalProjectHandler;
            this.settingsStorageManager = settingsStorageManager;
            this.grpcParser = grpcParser;
            this.manifestHandler = manifestHandler;
            this.settingsData = settingsData;
            this.resultModel = resultModel;
            this.dialogSelector = dialogSelector;
        }

        public async Task OpenOdbFolder(IDataSourceProvider projectHandler, Action resetTestCoverageAction)
        {
            string path = await projectHandler.GetOdbProjectFolder().ConfigureAwait(true);
            if (!string.IsNullOrEmpty(path))
            {
                string selectedPath = path;
                if (manifestHandler.IsManifestHandlingActive())
                {
                    await manifestHandler.UpdateOdbProject(path).ConfigureAwait(true);
                    logger.LogMessage("Successfully added ODB project files into project file", LogCategory.INFO);
                }

                await PerformLoadAction(resetTestCoverageAction, selectedPath).ConfigureAwait(false);
            }
        }

        public void HandleComponentImportFinishedEvent(ComponentsImportFinishedEvent obj, bool autoLoad, IProjectHandler projectHandlerToUse)
        {
            bool valuesAreBeingUsed = resultModel.CheckIfValuesAreBeingUsed();
            if (!autoLoad && !valuesAreBeingUsed)
            {
                bomData.ResetValues();
                bomHandler.OpenBomView();
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
                    bomHandler.OpenBomView();
                }
                else
                {
                    bomHandler.OpenBomView(true);
                }
            }
        }

        public async Task PerformLoadAction(Action resetTestCoverageAction, string selectedPath)
        {
            try
            {
                resetTestCoverageAction();
                settingsData.UseValues = false;
                eventService.Publish<SetBusyEvent>(new SetBusyEvent(true));
                eventService.Publish<ResetViewEvent>(new ResetViewEvent());
                AttributeList attributes = GeneralProjectHandler.GetAttributeList(settingsStorageManager.GetStorageContent());
                logger.LogMessage("Using of PCBInvestigator API for getting objects for path " + selectedPath + " started...", LogCategory.INFO);
                resultModel.Result = await grpcParser.GetParsedObjectsFromGrpcByZipFolder(selectedPath, settingsStorageManager.GetStorageContent().Steps, attributes.RDef, attributes.CDef, attributes.IDef, attributes.TDef, attributes.IcDef, attributes.ConDef).ConfigureAwait(true);
                var layerNames = GeneralProjectHandler.GetLayers(resultModel.Result);
                ((GeneralProjectHandler)generalProjectHandler).LogResults(layerNames, resultModel.Result);
                eventService.Publish<AddPcbObjectsEvent>(new AddPcbObjectsEvent(resultModel.Result.Components, resultModel.Result.Nets));
                eventService.Publish<SendLayersEvent>(new SendLayersEvent(layerNames));
            }
            catch (RpcException e)
            {
                logger.LogMessage("Error getting parsed objects: " + e.Message, LogCategory.ERROR);
                eventService.Publish<SetBusyEvent>(new SetBusyEvent(false));
            }
        }

        public async Task HandleExportJsonProject(IProjectHandler projectHandlerToUse)
        {
            if (resultModel.Result == null)
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

            eventService.Publish(new SetBusyEvent(true));
            await Task.Run(() =>
            {
                byte[] data = Encoding.ASCII.GetBytes(grpcParser.GetDataAsString(resultModel.Result));
                projectHandlerToUse.ExportJsonProjectFileContent(data, filePath);
                eventService.Publish<SetBusyEvent>(new SetBusyEvent(false));
            }).ConfigureAwait(false);
        }

        public async Task HandleImportJsonProject(IDataSourceProvider projectHandler, bool useSavingInProject, Action resetAction)
        {
            byte[] data = projectHandler.GetJsonProjectContent();
            if (data != null)
            {
                resetAction();
                eventService.Publish<SetBusyEvent>(new SetBusyEvent(true));
                eventService.Publish<ResetViewEvent>(new ResetViewEvent());
                AttributeList attributes = GeneralProjectHandler.GetAttributeList(settingsStorageManager.GetStorageContent());
                logger.LogMessage("Import of JSON data started for file...", LogCategory.INFO);
                await Task.Run(() =>
                {
                    resultModel.Result = grpcParser.ImportComponentsFromFile(
                    Encoding.ASCII.GetString(data),
                    attributes.RDef,
                    attributes.CDef,
                    attributes.IDef,
                    attributes.TDef,
                    attributes.IcDef,
                    attributes.ConDef);
                }).ConfigureAwait(true);
                if (resultModel.Result != null)
                {
                    if (useSavingInProject && manifestHandler.IsManifestHandlingActive())
                    {
                        if (manifestHandler.UpdateJsonProject(data))
                        {
                            logger.LogMessage("Successfully updated JSON project in project file", LogCategory.INFO);
                        }
                    }

                    settingsData.UseValues = resultModel.CheckIfValuesAreBeingUsed();
                    var layerNames = GeneralProjectHandler.GetLayers(resultModel.Result);
                    ((GeneralProjectHandler)generalProjectHandler).LogResults(layerNames, resultModel.Result);
                    eventService.Publish<AddPcbObjectsEvent>(new AddPcbObjectsEvent(resultModel.Result.Components, resultModel.Result.Nets));
                    eventService.Publish<SendLayersEvent>(new SendLayersEvent(layerNames));
                }
                else
                {
                    eventService.Publish<SetBusyEvent>(new SetBusyEvent(false));
                }
            }
        }
    }
}
