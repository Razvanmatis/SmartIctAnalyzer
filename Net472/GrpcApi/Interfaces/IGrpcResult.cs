using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.PcbInvestigator;

namespace GrpcApi.Interfaces
{
    public interface IGrpcResult
    {
        IList<IPinComponent> AllPins { get; }

        IList<INetComponent> AllNets { get; }

        IList<IPCBComponent> AllComponents { get; }
    }
}
