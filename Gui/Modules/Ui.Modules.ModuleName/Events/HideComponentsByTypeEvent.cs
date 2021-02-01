using System;
using System.Collections.Generic;
using System.Text;

namespace Ui.Modules.ModuleName.Events
{
    public class HideComponentsByTypeEvent
    {
        private Type type;

        public HideComponentsByTypeEvent(Type componentTypeToHide)
        {
            this.type = componentTypeToHide;
        }

        public Type ComponenTypeToHide
        {
            get
            {
                return type;
            }
        }
    }
}
