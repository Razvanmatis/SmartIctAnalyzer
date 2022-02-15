using ProMik.SmartIct.Interfaces.Container;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.Services.ManifestHandler.Interfaces
{
    public interface IProjectDataHandling
    {
        void ExportSettingsFileContent(byte[] data, string fileName, bool useOnlyJsonSettingsFileAsByteContent = false);

        void ExportJsonProjectFileContent(byte[] data, string fileName);

        string GetBomData();

        BomSettings GetBomSettings();
    }
}
