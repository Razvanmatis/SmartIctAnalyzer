using System;
using System.Threading.Tasks;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class DropDataSourceProvider : IDataSourceProvider
    {
        private readonly byte[] settingsContent;
        private readonly byte[] jsonProjectContent;
        private string odbPath;

        public DropDataSourceProvider(byte[] settingsContent, byte[] jsonProjectContent, string odbPath)
        {
            this.settingsContent = settingsContent;
            this.odbPath = odbPath;
            this.jsonProjectContent = jsonProjectContent;
        }

        public byte[] GetJsonProjectContent()
        {
            return jsonProjectContent;
        }

        public async Task<string> GetOdbProjectFolder()
        {
            return await Task.FromResult(odbPath).ConfigureAwait(true);
        }

        public byte[] GetSettingsFileContent()
        {
            return settingsContent;
        }
    }
}
