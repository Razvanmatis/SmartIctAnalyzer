using System.Collections.Generic;

namespace ProMik.SmartIct.Interfaces.PcbInvestigator.Implementations
{
    public class NetComponent : INetComponent
    {
        private readonly string netName;
        private IList<IPinComponent> pins;
        private IList<IPCBComponent> components;

        public NetComponent(string netName, IList<IPinComponent> pins, IList<IPCBComponent> components)
        {
            this.netName = netName;
            this.pins = pins;
            this.components = components;
        }

        public IList<IPinComponent> Pins
        {
            get { return pins; }
            set { pins = value; }
        }

        public IList<IPCBComponent> Components
        {
            get { return components; }
            set { components = value; }
        }

        public string NetName
        {
            get
            {
                return netName;
            }
        }
    }
}
