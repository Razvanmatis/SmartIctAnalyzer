using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.PcbInvestigator;

namespace Interfaces.PcbInvestigator
{
    public interface INetComponent
    {
        IList<IPinComponent> Pins { get; }

        IList<IPCBComponent> Components { get; }

        string NetName { get; }
    }
}
