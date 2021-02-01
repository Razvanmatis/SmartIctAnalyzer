using System;
using System.Collections.Generic;
using System.Text;
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
