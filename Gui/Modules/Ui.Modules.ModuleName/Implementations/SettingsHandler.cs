using System;
using System.Globalization;
using System.Windows.Forms;
using ProMik.Core.Interfaces.Events;
using ProMik.SmartIct.Interfaces.Gui;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.Views;

namespace Ui.Modules.ModuleName.Implementations
{
    public class SettingsHandler : ISettingsHandler
    {
        private readonly ISettingsStorageManager settingsStorageManager;
        private readonly ISettingsData settingsData;
        private readonly IManifestHandler manifestHandler;
        private readonly ILogger logger;
        private readonly IEventService eventService;
        private readonly IDialogSelector dialogSelector;
        private AllSettingsView allSettingsView;

        public SettingsHandler(
            ISettingsStorageManager settingsStorageManager,
            ISettingsData settingsData,
            IManifestHandler manifestHandler,
            ILogger logger,
            IDialogSelector dialogSelector,
            IEventService eventService)
        {
            this.settingsStorageManager = settingsStorageManager;
            this.settingsData = settingsData;
            this.eventService = eventService;
            this.manifestHandler = manifestHandler;
            this.logger = logger;
            this.dialogSelector = dialogSelector;
        }

        public void OpenAllSettingsView()
        {
            if (allSettingsView == null)
            {
                allSettingsView = new AllSettingsView(eventService);
            }

            allSettingsView.Top = (Screen.PrimaryScreen.Bounds.Height / 2) - (allSettingsView.Height / 2);
            allSettingsView.Left = (Screen.PrimaryScreen.Bounds.Width / 2) - (allSettingsView.Width / 2);
            allSettingsView.Show();
        }

        public void HandleImportSettings(IDataSourceProvider projectHandler, bool useManifestSaving)
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

        public void HandleExportSettings(IProjectHandler projectHandlerToUse)
        {
            string fileName = projectHandlerToUse.GetSettingsDestinationPath();
            if (string.IsNullOrEmpty(fileName))
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

        public void CloseAllSettingsView(CloseAllSettingsEvent obj)
        {
            if (allSettingsView != null)
            {
                allSettingsView.Hide();
            }
        }
    }
}
