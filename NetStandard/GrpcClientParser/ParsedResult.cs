using System;
using System.Collections.Generic;
using System.Text;
using GrpcClientParser.Interfaces;
using Interfaces.PcbInvestigator;

namespace GrpcClientParser
{
    public class ParsedResult : IParsedResult
    {
        private IList<IPCBComponent> components;
        private IList<INetComponent> nets;

        public ParsedResult(IList<IPCBComponent> components, IList<INetComponent> nets)
        {
            this.components = components;
            this.nets = nets;
        }

        public IList<IPCBComponent> Components
        {
            get
            {
                return components;
            }
        }

        public IList<INetComponent> Nets
        {
            get
            {
                return nets;
            }
        }
    }
}
