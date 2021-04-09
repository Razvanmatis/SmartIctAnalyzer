using System.Threading.Tasks;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface IDataSourceProvider
    {
        byte[] GetSettingsFileContent();

        byte[] GetJsonProjectContent();

        Task<string> GetOdbProjectFolder();
    }
}
