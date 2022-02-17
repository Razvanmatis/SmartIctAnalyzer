using System;
using System.Threading.Tasks;
using ProMik.SmartIct.Services.ManifestHandler.Interfaces;
using ProMik.SmartIct.Svf.SvfFileCreation.Interfaces;
using Ui.Modules.ModuleName.Helper;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IProjectHandler : IDataSourceProvider, ISvfExporter, IProjectDataHandling
    {
        string GetSettingsDestinationPath();

        string GetJsonExportPath();

        Task<bool> SaveProjectFile(
            Action<bool, bool, IProjectHandler> actionItemsSelect,
            string filename,
            IManifestHandler manifestHandler,
            IProjectLoadHandler projectLoadHandler,
            ISettingsHandler settingsHandler,
            IBomHandler bomHandler,
            ISvfHandler svfHandler);
    }
}
