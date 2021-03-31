using System.Collections.Generic;
using Interfaces.PcbInvestigator;
using ProMik.Core.Interfaces.Events;

namespace Ui.Modules.ModuleName.Events
{
    public class AddPcbObjectsEvent : EventPayload
    {
        private readonly IList<INetComponent> nets;
        private readonly IList<IPCBComponent> list;

        public AddPcbObjectsEvent(IList<IPCBComponent> list, IList<INetComponent> nets)
        {
            this.list = list;
            this.nets = nets;
        }

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
