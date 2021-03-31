using System.Collections.Generic;
using GrpcClientParser.Helper.JsonObjects;

namespace GrpcClientParser.Interfaces
{
    public interface IResultJson
    {
        IList<PinJson> Pins { get; set; }

        IList<ComponentJson> Components { get; set; }

        IList<NetJson> Nets { get; set; }
    }
}
