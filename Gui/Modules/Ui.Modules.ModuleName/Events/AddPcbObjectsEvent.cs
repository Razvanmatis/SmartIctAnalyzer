using System;
using System.Collections.Generic;
using System.Text;
using Interfaces.PcbInvestigator;

namespace Ui.Modules.ModuleName.Events
{
    public class AddPcbObjectsEvent
    {
        private IList<INetComponent> nets;
        private IList<IPCBComponent> list;

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
