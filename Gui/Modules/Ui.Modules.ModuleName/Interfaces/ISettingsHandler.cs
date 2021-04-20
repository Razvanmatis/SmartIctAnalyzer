using System;
using System.Collections.Generic;
using System.Text;
using Ui.Modules.ModuleName.Events;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface ISettingsHandler
    {
        void HandleImportSettings(IDataSourceProvider projectHandler, bool useManifestSaving);

        void HandleExportSettings(IProjectHandler projectHandlerToUse);

        void OpenAllSettingsView();

        void CloseAllSettingsView(CloseAllSettingsEvent obj);
    }
}
