using System.Collections.Generic;
using GrpcClientParser.Interfaces;
using Interfaces.PcbInvestigator;

namespace GrpcClientParser.Implementations
{
    public class ParsedResult : IParsedResult
    {
        private readonly IList<IPCBComponent> components;
        private readonly IList<INetComponent> nets;

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
