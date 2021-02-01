using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.PcbInvestigator;

namespace Interfaces.PCBApiObjects
{
    public class NetComponent : INetComponent
    {
        private IList<IPinComponent> pins;
        private IList<IPCBComponent> components;
        private string netName;

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
