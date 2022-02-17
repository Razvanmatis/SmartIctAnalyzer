using ProMik.SmartIct.Interfaces.PcbInvestigator.Implementations;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Enums;
using System.Collections.Generic;

namespace ProMik.SmartIct.JtagPinInformationExtractor.Components
{
    internal class DirectGndComponent : IPCBComponent
    {
        public IGeometricAttributes GeometricAttributes { get; }
        public IFunctionalAttributes FunctionalAttributes { get; } = new FunctionalAttributes(
            PCBObjectType.Component,
            "DIRECT_GND",
            "",
            "",
            "",
            "",
            x => false, 0);
        public IList<IPinComponent> Connections { get; }
        public PcbComponentType ComponentType { get; } = PcbComponentType.DirectGnd;
    }
}
