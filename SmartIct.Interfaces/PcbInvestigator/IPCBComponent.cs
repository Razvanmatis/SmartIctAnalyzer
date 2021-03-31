using System.Collections.Generic;

namespace Interfaces.PcbInvestigator
{
    public interface IPCBComponent
    {
        IGeometricAttributes GeometricAttributes { get; }

        IFunctionalAttributes FunctionalAttributes { get; }

        IList<IPinComponent> Connections { get; }
    }
}
