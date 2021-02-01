using System;
using System.Collections.Generic;
using System.Text;
using Ui.Modules.ModuleName.ViewModels;

namespace Ui.Modules.ModuleName.Events
{
    public class ShowConnectionsEvent
    {
        private ViewModelPCBComponentBase comp;
        private bool shouldShow;

        public ShowConnectionsEvent(ViewModelPCBComponentBase comp, bool shouldShow)
        {
            this.comp = comp;
            this.shouldShow = shouldShow;
        }

        public ViewModelPCBComponentBase Component
        {
            get
            {
                return comp;
            }
        }

        public bool ShouldShow
        {
            get
            {
                return shouldShow;
            }
        }
    }
}
