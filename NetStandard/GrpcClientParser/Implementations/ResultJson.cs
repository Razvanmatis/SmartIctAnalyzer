using System.Collections.Generic;
using GrpcClientParser.Helper.JsonObjects;
using GrpcClientParser.Interfaces;

namespace GrpcClientParser.Implementations
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
