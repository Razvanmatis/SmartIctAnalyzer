using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.Services.ManifestHandler.Interfaces
{
    public interface IManifestHandler : IDataSourceCollector, IManifestHandling, IProjectDataHandling, IsvfDataCollector
    {
    }
}
