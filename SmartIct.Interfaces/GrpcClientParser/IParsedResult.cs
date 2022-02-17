using System.Collections.Generic;
using ProMik.SmartIct.Interfaces.PcbInvestigator;

namespace ProMik.SmartIct.Interfaces.GrpcClientParser
{
    public interface IParsedResult
    {
        IList<IPCBComponent> Components { get; }

        IList<INetComponent> Nets { get; }

        int AmountSteps { get; }
    }
}
