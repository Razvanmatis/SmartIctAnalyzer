using System.Collections.Generic;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Enums;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Implementations;

namespace ProMik.SmartIct.PCBComponentParser.Implementations
{
    public class PCBCapacitor : PCBComponent
    {
        public PCBCapacitor(
            IGeometricAttributes geometricAttributes,
            IFunctionalAttributes functionalAttributes,
            IList<IPinComponent> connections)
            : base(geometricAttributes, functionalAttributes, connections)
        {
            ComponentType = PcbComponentType.Capacitor;
        }
    }
}
