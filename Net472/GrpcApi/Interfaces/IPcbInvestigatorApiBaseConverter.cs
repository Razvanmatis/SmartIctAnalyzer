using System.Collections.Generic;
using ProMik.SmartIct.Interfaces.PcbInvestigator;

namespace GrpcApi.Interfaces
{
    public interface IPcbInvestigatorApiBaseConverter
    {
        IList<NetGrpc> GetNetsGrpc(
            IList<IPinComponent> allPins, IList<IPCBComponent> allComponents, IList<INetComponent> allNets);

        IList<ComponentGrpc> GetComponentsGrpc(IList<IPCBComponent> allComponents, IList<IPinComponent> allPins);

        IList<PinGrpc> GetPinsGrpc(IList<IPinComponent> allPins, IList<INetComponent> allNets);
    }
}
