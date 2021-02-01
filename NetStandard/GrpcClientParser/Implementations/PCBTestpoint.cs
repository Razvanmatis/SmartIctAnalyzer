using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.PCBApiObjects;
using Interfaces.PcbInvestigator;

namespace GrpcClientParser.Implementations
{
    public class PCBTestpoint : PCBComponent
    {
        public PCBTestpoint(IGeometricAttributes geometricAttributes, IFunctionalAttributes functionalAttributes, IList<IPinComponent> connections)
            : base(geometricAttributes, functionalAttributes, connections)
        {
        }
    }
}
