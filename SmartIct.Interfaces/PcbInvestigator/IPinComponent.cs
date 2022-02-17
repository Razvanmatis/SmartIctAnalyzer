using System.Collections.Generic;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Enums;

namespace ProMik.SmartIct.Interfaces.PcbInvestigator
{
    public interface IPinComponent
    {
        IList<INetComponent> Nets { get; set; }

        IGeometricAttributes GeometricAttributes { get; }

        PinComponentType PinType { get; }

        string PinNumber { get; }
    }
}
