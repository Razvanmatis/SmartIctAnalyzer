using System.Collections.Generic;
using ProMik.SmartIct.Interfaces.GrpcClientParser;
using ProMik.SmartIct.Interfaces.PcbInvestigator;

namespace ProMik.SmartIct.PCBComponentParser.Implementations
{
    public class ParsedResult : IParsedResult
    {
        private readonly IList<IPCBComponent> components;
        private readonly IList<INetComponent> nets;
        private readonly int stepAmount;

        public ParsedResult(IList<IPCBComponent> components, IList<INetComponent> nets, int stepAmount)
        {
            this.components = components;
            this.nets = nets;
            this.stepAmount = stepAmount;
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

        public int AmountSteps
        {
            get
            {
                return stepAmount;
            }
        }
    }
}
