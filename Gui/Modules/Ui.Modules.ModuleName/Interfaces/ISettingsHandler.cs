using System;
using System.Collections.Generic;
using System.Text;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface ISettingsHandler
    {
        void HandleImportSettings(IDataSourceProvider projectHandler, bool useManifestSaving);

        void HandleExportSettings(IProjectHandler projectHandlerToUse);

        void OpenAllSettingsView();
    }
}
