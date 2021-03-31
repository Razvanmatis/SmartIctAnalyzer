using System.Collections.Generic;
using Interfaces.PcbInvestigator;
using Interfaces.PcbInvestigator.Implementations;

namespace GrpcClientParser.Implementations
{
    public class PCBResistor : PCBComponent
    {
        public PCBResistor(IGeometricAttributes geometricAttributes, IFunctionalAttributes functionalAttributes, IList<IPinComponent> connections)
            : base(geometricAttributes, functionalAttributes, connections)
        {
        }
    }
}
