using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using ProMik.Core.Interfaces.Events;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.PCBComponentParser.Interfaces;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Helper;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class ProjectLoadHandler : IProjectLoadHandler
    {
        private const string ALLSTEPS = "-1";
        private readonly IEventService eventService;
        private readonly ILogger logger;
        private readonly ISettingsStorageManager settingsStorageManager;
        private readonly IPCBComponentParser grpcParser;
        private readonly IManifestHandler manifestHandler;
        private readonly ISettingsData settingsData;
        private readonly IResultModel resultModel;
        private readonly IDialogSelector dialogSelector;
        private readonly IBomDataModel bomData;
        private readonly IBomHandler bomHandler;
        private readonly IProjectHandler generalProjectHandler;
        private string odbPath = string.Empty;

        public ProjectLoadHandler(
            IEventService eventService,
            ILogger logger,
            ISettingsStorageManager settingsStorageManager,
            IPCBComponentParser grpcParser,
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
            odbPath = await projectHandler.GetOdbProjectFolder().ConfigureAwait(true);
            if (await UpdateOdbProject().ConfigureAwait(false))
            {
                await PerformLoadAction(resetTestCoverageAction, odbPath).ConfigureAwait(false);
            }
        }

        public async Task<bool> UpdateOdbProject()
        {
            if (!string.IsNullOrEmpty(odbPath))
            {
                if (manifestHandler.IsManifestHandlingActive())
                {
                    await manifestHandler.UpdateOdbProject(odbPath).ConfigureAwait(false);
                    logger.LogMessage("Successfully added ODB project files into project file", LogCategory.INFO);
                }

                return true;
            }

            return false;
        }

        public void HandleComponentImportFinishedEvent(
            ComponentsImportFinishedEvent obj,
            bool autoLoad,
            IProjectHandler projectHandlerToUse)
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
                BomSettings bomSettings = manifestHandler.GetBomSettings();
                string bomFile = manifestHandler.GetBomData();
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
                await eventService.Publish<SetBusyEvent>(new SetBusyEvent(true)).ConfigureAwait(false);
                await eventService.Publish<ResetViewEvent>(new ResetViewEvent()).ConfigureAwait(false);
                AttributeList attributes = GeneralProjectHandler.GetAttributeList(settingsStorageManager.GetStorageContent());
                logger.LogMessage(
                    "Using of PCBInvestigator API for getting objects for path "
                    + selectedPath
                    + " started...",
                    LogCategory.INFO);
                resultModel.Result = await grpcParser.GetParsedObjectsFromGrpcByZipFolder(
                    selectedPath,
                    ALLSTEPS,
                    attributes.RDef,
                    attributes.CDef,
                    attributes.IDef,
                    attributes.TDef,
                    attributes.IcDef,
                    attributes.ConDef,
                    attributes.UseContains).ConfigureAwait(true);
                if (resultModel.Result.AmountSteps > 1)
                {
                    string steps = dialogSelector.OpenInputDialog(
                        "Please choose steps to be considered (1;2) (-1 = all)",
                        settingsData.Steps);
                    List<int> stepsToUse = GetStepsToUse(steps, resultModel.Result.AmountSteps);
                    if (string.IsNullOrEmpty(steps) || stepsToUse.Count == 0)
                    {
                        logger.LogMessage("No valid steps entered! Using -1 for all", LogCategory.WARNING);
                    }
                    else
                    {
                        if (resultModel.Result.AmountSteps > stepsToUse.Count)
                        {
                            RemoveAllUnsedStepObjects(stepsToUse);
                        }
                    }
                }

                var layerNames = GeneralProjectHandler.GetLayers(resultModel.Result);
                ((GeneralProjectHandler)generalProjectHandler).LogResults(layerNames, resultModel.Result);
                await eventService.Publish(new AddPcbObjectsEvent(
                    resultModel.Result.Components, resultModel.Result.Nets, layerNames)).ConfigureAwait(false);
                await eventService.Publish<SendLayersEvent>(new SendLayersEvent(layerNames)).ConfigureAwait(false);
            }
            catch (RpcException e)
            {
                logger.LogMessage("Error getting parsed objects: " + e.Message, LogCategory.ERROR);
                await eventService.Publish<SetBusyEvent>(new SetBusyEvent(false)).ConfigureAwait(false);
            }
        }

        public async Task HandleExportJsonProject(IProjectHandler projectHandlerToUse, bool triggerIsBusyEvent = true)
        {
            if (resultModel.Result == null)
            {
                return;
            }

            string filePath = projectHandlerToUse.GetJsonExportPath();
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            if (!filePath.ToLower(CultureInfo.CurrentCulture).EndsWith(".json", StringComparison.Ordinal))
            {
                filePath += ".json";
            }

            if (triggerIsBusyEvent)
            {
                eventService.Publish(new SetBusyEvent(true));
            }

            await Task.Run(() =>
            {
                byte[] data = Encoding.ASCII.GetBytes(grpcParser.GetDataAsString(resultModel.Result));
                projectHandlerToUse.ExportJsonProjectFileContent(data, filePath);
                if (triggerIsBusyEvent)
                {
                    eventService.Publish<SetBusyEvent>(new SetBusyEvent(false));
                }
            }).ConfigureAwait(false);
        }

        public async Task HandleImportJsonProject(
            IDataSourceProvider projectHandler,
            bool useSavingInProject,
            Action resetAction)
        {
            byte[] data = projectHandler.GetJsonProjectContent();
            if (data != null)
            {
                resetAction();
                await eventService.Publish<SetBusyEvent>(new SetBusyEvent(true)).ConfigureAwait(false);
                await eventService.Publish<ResetViewEvent>(new ResetViewEvent()).ConfigureAwait(false);
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
                    attributes.ConDef,
                    attributes.UseContains);
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
                    await eventService.Publish(new AddPcbObjectsEvent(
                        resultModel.Result.Components, resultModel.Result.Nets, layerNames)).ConfigureAwait(false);
                    await eventService.Publish<SendLayersEvent>(new SendLayersEvent(layerNames)).ConfigureAwait(false);
                }
                else
                {
                    await eventService.Publish<SetBusyEvent>(new SetBusyEvent(false)).ConfigureAwait(false);
                }
            }
        }

        private void RemoveAllUnsedStepObjects(List<int> stepsToUse)
        {
            List<IPCBComponent> compsToDelete = new List<IPCBComponent>(resultModel.Result.Components.Where(comp =>
                !stepsToUse.Contains(comp.FunctionalAttributes.StepNo)));
            List<INetComponent> netsToDelete = new List<INetComponent>();
            foreach (var comp in compsToDelete)
            {
                foreach (var pin in comp.Connections)
                {
                    foreach (var net in pin.Nets)
                    {
                        bool shouldBeDeleted = true;
                        foreach (var compInner in net.Components)
                        {
                            if (!compsToDelete.Contains(compInner))
                            {
                                shouldBeDeleted = false;
                                break;
                            }
                        }

                        if (shouldBeDeleted)
                        {
                            netsToDelete.Add(net);
                        }
                    }
                }
            }

            compsToDelete.ForEach(comp => resultModel.Result.Components.Remove(comp));
            netsToDelete.ForEach(net => resultModel.Result.Nets.Remove(net));
        }

        private List<int> GetStepsToUse(string steps, int amountSteps)
        {
            List<int> list = new List<int>();
            string[] numbers = steps.Split(";");
            foreach (var num in numbers)
            {
                if (int.TryParse(num, out int res))
                {
                    if (res >= -1)
                    {
                        if (res == -1)
                        {
                            for (int idx = 1; idx <= amountSteps; idx++)
                            {
                                list.Add(idx);
                            }

                            break;
                        }
                        else
                        {
                            list.Add(res);
                        }
                    }
                    else
                    {
                        logger.LogMessage("Invalid step defined! Should be greater than 0, but was: " + res, LogCategory.ERROR);
                    }
                }
                else
                {
                    logger.LogMessage("Error at parsing step number: " + num, LogCategory.ERROR);
                }
            }

            return list;
        }
    }
}
