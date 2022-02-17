using System;
using System.Globalization;
using System.Threading.Tasks;
using ProMik.Core.Interfaces.Events;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class ProjectFileHandler : IProjectFileHandler
    {
        private readonly IManifestHandler manifestHandler;
        private readonly ISettingsHandler settingsHandler;
        private readonly IProjectLoadHandler projectLoadHandler;
        private readonly IDialogSelector dialogSelector;
        private readonly IProjectHandler generalProjectHandler;
        private readonly ILogger logger;
        private readonly IEventService eventService;
        private readonly IBomHandler bomHandler;
        private readonly ISvfHandler svfHandler;

        public ProjectFileHandler(
            IManifestHandler manifestHandler,
            ISettingsHandler settingsHandler,
            IProjectLoadHandler projectLoadHandler,
            IProjectHandler generalProjectHandler,
            IDialogSelector dialogSelector,
            ILogger logger,
            IEventService eventService,
            IBomHandler bomHandler,
            ISvfHandler svfHandler)
        {
            this.eventService = eventService;
            this.manifestHandler = manifestHandler;
            this.generalProjectHandler = generalProjectHandler;
            this.dialogSelector = dialogSelector;
            this.logger = logger;
            this.settingsHandler = settingsHandler;
            this.projectLoadHandler = projectLoadHandler;
            this.svfHandler = svfHandler;
            this.bomHandler = bomHandler;
        }

        public void CloseProjectAction(Action resetAllAction, Action<bool, bool, IProjectHandler> actionItemsSelect)
        {
            if (manifestHandler.IsManifestHandlingActive())
            {
                resetAllAction();
                logger.LogMessage("Project file usage closed...", LogCategory.INFO);
            }
            else
            {
                logger.LogMessage("No project file usage was active!", LogCategory.WARNING);
            }

            actionItemsSelect(false, false, generalProjectHandler);
        }

        public void CreateProjectFileBase(Action resetAllAction, Action<bool, bool, IProjectHandler> actionItemsSelect)
        {
            resetAllAction();
            actionItemsSelect(false, false, generalProjectHandler);
            CreateProjectFileDetail();
            actionItemsSelect(false, true, manifestHandler as IProjectHandler);
        }

        public async Task OpenProjectFile(
            string path,
            Action resetAllAction,
            Action resetTestCoverageAction,
            Action<bool, bool, IProjectHandler> setItemsAction)
        {
            setItemsAction(false, false, generalProjectHandler);
            bool autoLoad = false;
            resetAllAction();
            IProjectHandler handler = manifestHandler as IProjectHandler;
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            await eventService.Publish(new SetBusyEvent(true)).ConfigureAwait(false);
            manifestHandler.SetPathAndManifestOfZipfile(path, false);
            if (!manifestHandler.IsManifestHandlingActive())
            {
                await eventService.Publish(new SetBusyEvent(false)).ConfigureAwait(false);
                return;
            }

            byte[] settingsFile = manifestHandler.GetSettingsFileContent();
            if (settingsFile != null)
            {
                settingsHandler.HandleImportSettings(handler, false);
            }

            byte[] jsonPathOfProject = manifestHandler.GetJsonProjectContent();
            if (jsonPathOfProject != null)
            {
                autoLoad = true;
                await projectLoadHandler.HandleImportJsonProject(handler, false, resetTestCoverageAction).ConfigureAwait(false);
            }
            else
            {
                string odbPath = await manifestHandler.GetOdbProjectFolder().ConfigureAwait(true);
                if (!string.IsNullOrEmpty(odbPath))
                {
                    string selectedPath = odbPath;
                    autoLoad = true;
                    await projectLoadHandler.PerformLoadAction(resetTestCoverageAction, selectedPath).ConfigureAwait(false);
                }
            }

            setItemsAction(autoLoad, true, manifestHandler as IProjectHandler);
            await eventService.Publish(new SetBusyEvent(false)).ConfigureAwait(false);
        }

        public async Task<bool> SaveProjectFile(IProjectHandler projectHandlerToUse, Action<bool, bool, IProjectHandler> actionItemsSelect)
        {
            if (OpenFileSelectionDialog(out string filename))
            {
                return await projectHandlerToUse.SaveProjectFile(
                    actionItemsSelect,
                    filename,
                    manifestHandler,
                    projectLoadHandler,
                    settingsHandler,
                    bomHandler,
                    svfHandler).ConfigureAwait(false);
            }

            return false;
        }

        private bool CreateProjectFileDetail()
        {
            if (!OpenFileSelectionDialog(out string fileName))
            {
                return false;
            }

            manifestHandler.SetPathAndManifestOfZipfile(fileName, true);
            logger.LogMessage("Successfully created project file: " + fileName, LogCategory.INFO);
            return true;
        }

        private bool OpenFileSelectionDialog(out string fileName)
        {
            if (!dialogSelector.OpenGenericDialog(
                DialogType.SAVEFILE,
                TextRessources.SelectProjectFileToSave,
                "No valid target to save selected!",
                out fileName))
            {
                return false;
            }

            if (!fileName.ToLower(CultureInfo.CurrentCulture).EndsWith(".svfproj", StringComparison.OrdinalIgnoreCase))
            {
                fileName += ".svfproj";
            }

            return true;
        }
    }
}
