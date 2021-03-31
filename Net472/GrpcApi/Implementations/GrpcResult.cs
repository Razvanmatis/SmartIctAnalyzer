using System.Collections.Generic;
using GrpcApi.Interfaces;
using Interfaces.PcbInvestigator;

namespace GrpcApi.Implementations
{
    public class GrpcResult : IGrpcResult
    {
        private readonly IList<IPinComponent> allPins;
        private readonly IList<INetComponent> allNets;
        private readonly IList<IPCBComponent> allComponents;

        public GrpcResult(IList<IPinComponent> allPins, IList<INetComponent> allNets, IList<IPCBComponent> allComponents)
        {
            this.allComponents = allComponents;
            this.allNets = allNets;
            this.allPins = allPins;
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
    }
}
