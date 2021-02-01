using System;
using System.Collections.Generic;
using System.Text;
using GrpcClientParser.Interfaces;

namespace GrpcClientParser.Helper.JsonObjects
{
    public class ResultJson : IResultJson
    {
        public ResultJson(IList<PinJson> pins, IList<ComponentJson> components, IList<NetJson> nets)
        {
            Pins = pins;
            Components = components;
            Nets = nets;
        }

        public IList<PinJson> Pins { get; set; }

        public IList<ComponentJson> Components { get; set; }

        public IList<NetJson> Nets { get; set; }
    }
}
