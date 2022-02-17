using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using ProMik.SmartIct.Interfaces.Gui;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class DropHandler : IDropHandler
    {
        private readonly ISettingsStorageManager settingsStorageManager;
        private readonly ISettingsHandler settingsHandler;
        private readonly IProjectHandler generalProjectHandler;
        private readonly ILogger logger;
        private readonly IProjectLoadHandler projectLoadHandler;
        private readonly IProjectFileHandler projectFileHandler;

        public DropHandler(
            ISettingsStorageManager settingsStorageManager,
            ISettingsHandler settingsHandler,
            IProjectHandler generalProjectHandler,
            ILogger logger,
            IProjectLoadHandler projectLoadHandler,
            IProjectFileHandler projectFileHandler)
        {
            this.settingsStorageManager = settingsStorageManager;
            this.settingsHandler = settingsHandler;
            this.projectFileHandler = projectFileHandler;
            this.logger = logger;
            this.projectLoadHandler = projectLoadHandler;
            this.generalProjectHandler = generalProjectHandler;
        }

        public async Task HandleDropEventMethod(
            HandleDropEvent obj,
            Action resetAllAction,
            Action resetTestCoverageAction,
            Action<bool, bool, IProjectHandler> setItemsAction)
        {
            string fileName = obj.FileName.ToLower(CultureInfo.CurrentCulture);
            if (fileName.EndsWith(
                ".json", StringComparison.InvariantCulture)
                || fileName.EndsWith(".txt", StringComparison.InvariantCulture)
                || fileName.EndsWith(".db", StringComparison.InvariantCulture))
            {
                byte[] settingsContent = settingsStorageManager.IsValidSettingsFile(obj.FileName);
                if (settingsContent != null)
                {
                    settingsHandler.HandleImportSettings(new DropDataSourceProvider(settingsContent, null, string.Empty), true);
                }
                else
                {
                    byte[] content = ((GeneralProjectHandler)generalProjectHandler).GetBytesOfFile(obj.FileName);
                    if (content != null)
                    {
                        await projectLoadHandler.HandleImportJsonProject(
                            new DropDataSourceProvider(null, content, string.Empty), true, resetTestCoverageAction)
                            .ConfigureAwait(false);
                    }
                    else
                    {
                        logger.LogMessage("Not supported file dropped into application!", LogCategory.ERROR);
                    }
                }
            }
            else if (!Directory.Exists(obj.FileName))
            {
                await projectFileHandler.OpenProjectFile(
                    obj.FileName,
                    resetAllAction,
                    resetTestCoverageAction,
                    setItemsAction).ConfigureAwait(false);
            }
            else
            {
                await projectLoadHandler.OpenOdbFolder(
                    new DropDataSourceProvider(null, null, obj.FileName), resetTestCoverageAction).ConfigureAwait(false);
            }
        }
    }
}
