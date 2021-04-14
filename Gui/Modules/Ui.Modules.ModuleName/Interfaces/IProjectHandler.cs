using SVFHelper.Interfaces;
using Ui.Modules.ModuleName.Helper;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IProjectHandler : IDataSourceProvider, ISvfExporter
    {
        void ExportSettingsFileContent(byte[] data, string fileName);

        void ExportJsonProjectFileContent(byte[] data, string fileName);

        string GetBomData();

        BomSettings GetBomSettings();
    }
}
