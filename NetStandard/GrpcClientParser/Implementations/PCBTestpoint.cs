using System.Collections.Generic;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Enums;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Implementations;

namespace ProMik.SmartIct.PCBComponentParser.Implementations
{
    public class PCBTestpoint : PCBComponent
    {
        public PCBTestpoint(
            IGeometricAttributes geometricAttributes,
            IFunctionalAttributes functionalAttributes,
            IList<IPinComponent> connections)
            : base(geometricAttributes, functionalAttributes, connections)
        {
            ComponentType = PcbComponentType.TestPoint;
        }
    }
}
