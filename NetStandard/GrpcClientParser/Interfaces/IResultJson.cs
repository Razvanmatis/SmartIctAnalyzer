using System.Collections.Generic;
using ProMik.SmartIct.PCBComponentParser.Helper.JsonObjects;

namespace ProMik.SmartIct.PCBComponentParser.Interfaces
{
    public interface IResultJson
    {
        IList<PinJson> Pins { get; set; }

        IList<ComponentJson> Components { get; set; }

        IList<NetJson> Nets { get; set; }

        int StepAmount { get; }
    }
}
