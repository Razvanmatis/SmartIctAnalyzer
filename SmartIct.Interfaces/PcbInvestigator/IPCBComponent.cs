using System.Collections.Generic;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Enums;

namespace ProMik.SmartIct.Interfaces.PcbInvestigator
{
    public interface IPCBComponent
    {
        IGeometricAttributes GeometricAttributes { get; }

        IFunctionalAttributes FunctionalAttributes { get; }

        IList<IPinComponent> Connections { get; }

        PcbComponentType ComponentType { get; }
    }
}
