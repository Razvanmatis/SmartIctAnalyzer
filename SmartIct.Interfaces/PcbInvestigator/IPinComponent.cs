using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.PcbInvestigator.Enums;

namespace Interfaces.PcbInvestigator
{
    public interface IPinComponent
    {
        IList<INetComponent> Nets { get; set; }

        IGeometricAttributes GeometricAttributes { get; }

        PinComponentType PinType { get; }
    }
}
