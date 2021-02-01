using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Ui.Modules.ModuleName.Helper;

namespace Ui.Modules.ModuleName.Interfaces
{
    public interface ISettingsStorageManager
    {
        void SaveStorageContent(ISettingsStorageContent content);

        ISettingsStorageContent GetStorageContent();

        void ImportStorageContent(string filePath);

        void ExportStorageContent(string filePath);
    }
}
