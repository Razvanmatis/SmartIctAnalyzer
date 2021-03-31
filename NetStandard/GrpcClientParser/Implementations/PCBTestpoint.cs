using System.Collections.Generic;
using Interfaces.PcbInvestigator;
using Interfaces.PcbInvestigator.Implementations;

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
