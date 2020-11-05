using System;
using System.Collections.Generic;
using System.Text;
using PCBInvestAPI.DataModel.OutLineObjects;

namespace PCBInvestAPI.DataModel
{
    public class FileContent
    {
        public string FileName { get; set; }
        public string FileFormat { get; set; }
        public string Unit { get; set; } = "mm";

        public List<BaseOutlineObject> PCBOutline { get; set; }
        public List<TestPoint> TestPoints { get; set; }
        public List<Component> Componenten { get; set; }

    }
}
