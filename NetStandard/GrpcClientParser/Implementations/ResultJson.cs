using System.Collections.Generic;
using ProMik.SmartIct.PCBComponentParser.Helper.JsonObjects;
using ProMik.SmartIct.PCBComponentParser.Interfaces;

namespace ProMik.SmartIct.PCBComponentParser.Implementations
{
    public class ResultJson : IResultJson
    {
        public ResultJson(IList<PinJson> pins, IList<ComponentJson> components, IList<NetJson> nets, int stepAmount)
        {
            Pins = pins;
            Components = components;
            Nets = nets;
            StepAmount = stepAmount;
        }

        public IList<PinJson> Pins { get; set; }

        public IList<ComponentJson> Components { get; set; }

        public IList<NetJson> Nets { get; set; }

        public int StepAmount { get; }
    }
}
