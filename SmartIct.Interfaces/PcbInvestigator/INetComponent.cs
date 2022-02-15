using System.Collections.Generic;

namespace ProMik.SmartIct.Interfaces.PcbInvestigator
{
    public interface INetComponent
    {
        IList<IPinComponent> Pins { get; }

        IList<IPCBComponent> Components { get; }

        string NetName { get; }
    }
}
