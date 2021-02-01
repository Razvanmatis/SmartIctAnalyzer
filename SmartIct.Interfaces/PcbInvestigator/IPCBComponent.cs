using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.PcbInvestigator;

namespace Interfaces.PcbInvestigator
{
    public interface IPCBComponent
    {
        IGeometricAttributes GeometricAttributes { get; }

        IFunctionalAttributes FunctionalAttributes { get; }

        IList<IPinComponent> Connections { get; }
    }
}
