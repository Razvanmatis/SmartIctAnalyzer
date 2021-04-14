using System.Collections.Generic;

namespace SVFHelper.Interfaces
{
    public interface ISvfExporter
    {
        void ExportSvfFiles(List<ISvfData> data);
    }
}
