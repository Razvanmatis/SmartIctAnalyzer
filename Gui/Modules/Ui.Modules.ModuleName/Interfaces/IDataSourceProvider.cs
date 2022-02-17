using System.Threading.Tasks;
using ProMik.SmartIct.Services.ManifestHandler.Interfaces;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IDataSourceProvider : IDataSourceCollector
    {
        Task<string> GetOdbProjectFolder();
    }
}
