using ProMik.SmartIct.Interfaces.PcbInvestigator.Implementations;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.JtagPinInformationExtractor.Components
{
    internal class DirectPowerComponent : IPCBComponent
    {
        public IGeometricAttributes GeometricAttributes { get; }
        public IFunctionalAttributes FunctionalAttributes { get; } = new FunctionalAttributes(
            PCBObjectType.Component,
            "DIRECT_POWER",
            "",
            "",
            "",
            "",
            x => false, 0);
        public IList<IPinComponent> Connections { get; }
        public PcbComponentType ComponentType { get; } = PcbComponentType.DirectPower;
    }
}
