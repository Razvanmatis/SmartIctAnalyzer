using System.Collections.Generic;
using ProMik.Core.Interfaces.Events;
using ProMik.SmartIct.Interfaces.PcbInvestigator;

namespace Ui.Modules.ModuleName.Events
{
    public class AddPcbObjectsEvent : EventPayload
    {
        private readonly IList<INetComponent> nets;
        private readonly IList<IPCBComponent> list;
        private readonly List<string> layerNames;

        public AddPcbObjectsEvent(IList<IPCBComponent> list, IList<INetComponent> nets, List<string> layerNames)
        {
            this.list = list;
            this.nets = nets;
            this.layerNames = layerNames;
        }

        public List<string> LayerNames { get => layerNames; }

        public IList<INetComponent> Nets
        {
            get
            {
                return nets;
            }
        }

        public IList<IPCBComponent> ComponentList
        {
            get
            {
                return list;
            }
        }
    }
}
