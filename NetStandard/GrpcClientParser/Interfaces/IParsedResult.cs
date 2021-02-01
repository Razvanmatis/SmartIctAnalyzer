using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.PcbInvestigator;

namespace GrpcClientParser.Interfaces
{
    public interface IParsedResult
    {
        IList<IPCBComponent> Components { get; }

        IList<INetComponent> Nets { get; }
    }
}
