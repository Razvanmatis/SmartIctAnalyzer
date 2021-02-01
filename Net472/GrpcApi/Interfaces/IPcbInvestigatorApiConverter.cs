using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.PcbInvestigator;
using PCBI.Automation.Interfaces;

namespace GrpcApi.Interfaces
{
    public interface IPcbInvestigatorApiConverter
    {
        IGrpcResult GetConvertedObjects(IList<InterfaceCMPObject> pcbObjects, IList<InterfaceNet> allNets);

        IList<NetGrpc> GetNetsGrpc(IList<IPinComponent> allPins, IList<IPCBComponent> allComponents, IList<INetComponent> allNets);

        IList<ComponentGrpc> GetComponentsGrpc(IList<IPCBComponent> allComponents, IList<IPinComponent> allPins);

        IList<PinGrpc> GetPinsGrpc(IList<IPinComponent> allPins, IList<INetComponent> allNets);
    }
}
