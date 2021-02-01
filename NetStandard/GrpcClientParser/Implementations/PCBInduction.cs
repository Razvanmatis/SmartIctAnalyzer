using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.PCBApiObjects;
using Interfaces.PcbInvestigator;

namespace GrpcClientParser.Implementations
{
    public class PCBInduction : PCBComponent
    {
        public PCBInduction(IGeometricAttributes geometricAttributes, IFunctionalAttributes functionalAttributes, IList<IPinComponent> connections)
            : base(geometricAttributes, functionalAttributes, connections)
        {
        }
    }
}
