using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.PcbInvestigator;
using Interfaces.PcbInvestigator.Enums;

namespace Interfaces.PCBApiObjects
{
    public class PinComponent : IPinComponent
    {
        private IList<INetComponent> nets;
        private IGeometricAttributes geometricAttributes;
        private PinComponentType pinType;
        private string pinNumber;

        public PinComponent(
            IGeometricAttributes geometricAttributes,
            PinComponentType pinType,
            IList<INetComponent> nets,
            string pinNumber)
        {
            this.pinNumber = pinNumber;
            this.geometricAttributes = geometricAttributes;
            this.pinType = pinType;
            this.nets = nets;
        }

        public IList<INetComponent> Nets
        {
            get { return nets; }
            set { nets = value; }
        }

        public IGeometricAttributes GeometricAttributes
        {
            get
            {
                return geometricAttributes;
            }
        }

        public string PinNumber
        {
            get
            {
                return pinNumber;
            }
        }

        public PinComponentType PinType
        {
            get
            {
                return pinType;
            }
        }
    }
}
