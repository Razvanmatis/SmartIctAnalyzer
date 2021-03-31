using System.Collections.Generic;

namespace Interfaces.PcbInvestigator.Implementations
{
    public class PCBComponent : IPCBComponent
    {
        private readonly IGeometricAttributes geometricAttributes;
        private readonly IFunctionalAttributes functionalAttributes;
        private readonly IList<IPinComponent> connections;

        public PCBComponent(IGeometricAttributes geometricAttributes, IFunctionalAttributes functionalAttributes, IList<IPinComponent> connections)
        {
            this.connections = connections;
            this.geometricAttributes = geometricAttributes;
            this.functionalAttributes = functionalAttributes;
            IsVisible = false;
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
    }
}
