using System;
using System.Collections.Generic;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Enums;

namespace ProMik.SmartIct.Interfaces.PcbInvestigator.Implementations
{
    public class PCBComponent : IPCBComponent
    {
        private readonly IGeometricAttributes geometricAttributes;
        private readonly IFunctionalAttributes functionalAttributes;
        private readonly IList<IPinComponent> connections;

        public PCBComponent(
            IGeometricAttributes geometricAttributes,
            IFunctionalAttributes functionalAttributes,
            IList<IPinComponent> connections)
        {
            this.connections = connections;
            this.geometricAttributes = geometricAttributes;
            this.functionalAttributes = functionalAttributes;
            IsVisible = false;
            ComponentType = PcbComponentType.Undefined;
        }

        public IGeometricAttributes GeometricAttributes
        {
            get
            {
                return geometricAttributes;
            }
        }

        public IFunctionalAttributes FunctionalAttributes
        {
            get
            {
                return functionalAttributes;
            }
        }

        public IList<IPinComponent> Connections
        {
            get
            {
                return connections;
            }
        }

        public bool IsVisible
        {
            get;
            set;
        }

        public PcbComponentType ComponentType { get; protected set; }
    }
}
