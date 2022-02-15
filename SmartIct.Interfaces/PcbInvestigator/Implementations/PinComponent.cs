using System.Collections.Generic;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Enums;

namespace ProMik.SmartIct.Interfaces.PcbInvestigator.Implementations
{
    public class PinComponent : IPinComponent
    {
        private readonly IGeometricAttributes geometricAttributes;
        private readonly PinComponentType pinType;
        private readonly string pinNumber;
        private IList<INetComponent> nets;

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
