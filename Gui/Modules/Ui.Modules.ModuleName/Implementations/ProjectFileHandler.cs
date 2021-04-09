using System;
using System.Threading.Tasks;
using Interfaces.Gui;
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

        public ProjectFileHandler(
            IManifestHandler manifestHandler,
            ISettingsHandler settingsHandler,
            IProjectLoadHandler projectLoadHandler,
            IProjectHandler generalProjectHandler,
            IDialogSelector dialogSelector,
            ILogger logger)
        {
            this.manifestHandler = manifestHandler;
            this.generalProjectHandler = generalProjectHandler;
            this.dialogSelector = dialogSelector;
            this.logger = logger;
            this.settingsHandler = settingsHandler;
            this.projectLoadHandler = projectLoadHandler;
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

        public void CreateProjectFile(Action resetAllAction, Action<bool, bool, IProjectHandler> actionItemsSelect)
        {
            resetAllAction();
            actionItemsSelect(false, false, generalProjectHandler);
            if (!dialogSelector.OpenGenericDialog(DialogType.SAVEFILE, TextRessources.SelectProjectFileToSave, "No valid target to save selected!", out string fileName))
            {
                return;
            }

            manifestHandler.SetPathAndManifestOfZipfile(fileName, true);
            logger.LogMessage("Successfully created project file: " + fileName, LogCategory.INFO);
            actionItemsSelect(false, true, manifestHandler as IProjectHandler);
        }

        public async Task OpenProjectFile(string path, Action resetAllAction, Action resetTestCoverageAction, Action<bool, bool, IProjectHandler> setItemsAction)
        {
            setItemsAction(false, false, generalProjectHandler);
            bool autoLoad = false;
            resetAllAction();
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

            byte[] settingsFile = manifestHandler.GetSettingsFile();
            if (settingsFile != null)
            {
                settingsHandler.HandleImportSettings(handler, false);
            }

            byte[] jsonPathOfProject = manifestHandler.GetJsonProjectFile();
            if (jsonPathOfProject != null)
            {
                autoLoad = true;
                await projectLoadHandler.HandleImportJsonProject(handler, false, resetTestCoverageAction).ConfigureAwait(false);
            }
            else
            {
                string odbPath = await manifestHandler.GetOdbProjectPath().ConfigureAwait(true);
                if (!string.IsNullOrEmpty(odbPath))
                {
                    string selectedPath = odbPath;
                    autoLoad = true;
                    await projectLoadHandler.PerformLoadAction(resetTestCoverageAction, selectedPath).ConfigureAwait(false);
                }
            }

            setItemsAction(autoLoad, true, manifestHandler as IProjectHandler);
        }
    }
}
