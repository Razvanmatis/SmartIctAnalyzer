using System.Collections.Generic;
using Interfaces.PcbInvestigator;
using Interfaces.PcbInvestigator.Implementations;

namespace GrpcClientParser.Implementations
{
    public class PCBConnector : PCBComponent
    {
        public PCBConnector(IGeometricAttributes geometricAttributes, IFunctionalAttributes functionalAttributes, IList<IPinComponent> connections)
            : base(geometricAttributes, functionalAttributes, connections)
        {
        }
    }
}
