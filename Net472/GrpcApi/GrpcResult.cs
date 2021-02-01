using System;
using System.Collections.Generic;
using System.Text;
using GrpcApi.Interfaces;
using Interfaces.PcbInvestigator;

namespace GrpcApi
{
    public class GrpcResult : IGrpcResult
    {
        private IList<IPinComponent> allPins;
        private IList<INetComponent> allNets;
        private IList<IPCBComponent> allComponents;

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
