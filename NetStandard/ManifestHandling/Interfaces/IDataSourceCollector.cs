using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.Services.ManifestHandler.Interfaces
{
    public interface IDataSourceCollector
    {
        byte[] GetSettingsFileContent(bool useJsonSettingsFile = false);

        byte[] GetJsonProjectContent();
    }
}
