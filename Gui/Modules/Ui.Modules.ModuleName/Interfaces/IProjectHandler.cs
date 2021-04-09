using System.Collections.Generic;
using System.Threading.Tasks;
using SVFHelper.Interfaces;
using Ui.Modules.ModuleName.Helper;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IProjectHandler : IDataSourceProvider
    {
        void ExportSettingsFileContent(byte[] data, string fileName);

        void ExportJsonProjectFileContent(byte[] data, string fileName);

        string GetBomData();

        BomSettings GetBomSettings();

        void ExportSvfFiles(List<ISvfData> data);
    }
}
