using System;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.Implementations
{
    public class DropDataSourceProvider : IDataSourceProvider
    {
        private byte[] settingsContent;
        private byte[] jsonProjectContent;

        public DropDataSourceProvider(byte[] settingsContent, byte[] jsonProjectContent)
        {
            this.settingsContent = settingsContent;
            this.jsonProjectContent = jsonProjectContent;
        }

        public byte[] GetJsonProjectContent()
        {
            return jsonProjectContent;
        }

        public byte[] GetSettingsFileContent()
        {
            return settingsContent;
        }
    }
}
