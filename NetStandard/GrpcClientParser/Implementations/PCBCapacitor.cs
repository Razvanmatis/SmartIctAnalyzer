using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.PCBApiObjects;
using Interfaces.PcbInvestigator;

namespace GrpcClientParser.Implementations
{
    public class PCBCapacitor : PCBComponent
    {
        public PCBCapacitor(IGeometricAttributes geometricAttributes, IFunctionalAttributes functionalAttributes, IList<IPinComponent> connections)
            : base(geometricAttributes, functionalAttributes, connections)
        {
        }
    }
}
