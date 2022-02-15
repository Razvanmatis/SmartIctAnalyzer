using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProMik.SmartIct.Services.ManifestHandler.Interfaces
{
    public interface IsvfDataCollector
    {
        Stream GetBsdlContenAsStream(out string fileName, string jtagDevice = "");

        void ExportSvfFiles(List<ISvfData> data);
    }
}
