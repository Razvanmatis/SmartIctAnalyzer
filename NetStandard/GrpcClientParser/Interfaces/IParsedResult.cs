using System.Collections.Generic;
using Interfaces.PcbInvestigator;

namespace GrpcClientParser.Interfaces
{
    public interface IParsedResult
    {
        IList<IPCBComponent> Components { get; }

        IList<INetComponent> Nets { get; }
    }
}
