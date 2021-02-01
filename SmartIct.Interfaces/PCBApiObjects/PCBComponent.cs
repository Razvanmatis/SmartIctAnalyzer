using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Interfaces.PcbInvestigator;

namespace Interfaces.PCBApiObjects
{
    public class PCBComponent : IPCBComponent
    {
        private IGeometricAttributes geometricAttributes;
        private IFunctionalAttributes functionalAttributes;
        private IList<IPinComponent> connections;

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
