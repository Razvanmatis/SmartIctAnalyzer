using System.Collections.Generic;

namespace Interfaces.PcbInvestigator
{
    public interface INetComponent
    {
        IList<IPinComponent> Pins { get; }

        IList<IPCBComponent> Components { get; }

        string NetName { get; }
    }
}
