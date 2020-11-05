using System;
using System.Collections.Generic;
using Uniconverter.Core.DataModel.OutLineObjects;

namespace Uniconverter.Core.DataModel
{


    /// <summary>
    /// FileContent for CSV, UNI, Farbmaster CAD, ASC, brd , tgz_odb, ccz(cc)
    /// </summary>
    public class FileContent
    {
        public string FileName { get; set; }
        public string FileFormat { get; set; }
        public string Unit { get; set; }

        public List<BaseOutlineObject> PCBOutline { get; set; }
        public List<TestPoint> TestPoints { get; set; }
        public List<Component> Componenten { get; set; }

    }
}
