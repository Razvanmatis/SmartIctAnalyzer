using System.Collections.Generic;
using GrpcApi.Interfaces;
using ProMik.SmartIct.Interfaces.PcbInvestigator;

namespace GrpcApi.Implementations
{
    public class GrpcResult : IGrpcResult
    {
        private readonly IList<IPinComponent> allPins;
        private readonly IList<INetComponent> allNets;
        private readonly IList<IPCBComponent> allComponents;
        private readonly int stepAmount;

        public GrpcResult(
            IList<IPinComponent> allPins, IList<INetComponent> allNets, IList<IPCBComponent> allComponents, int stepAmount)
        {
            this.allComponents = allComponents;
            this.allNets = allNets;
            this.allPins = allPins;
            this.stepAmount = stepAmount;
        }

        public IList<IPinComponent> AllPins
        {
            get
            {
                return allPins;
            }
        }

        public IList<INetComponent> AllNets
        {
            get
            {
                return allNets;
            }
        }

        public IList<IPCBComponent> AllComponents
        {
            get
            {
                return allComponents;
            }
        }

        public int StepAmount
        {
            get
            {
                return stepAmount;
            }
        }
    }
}
