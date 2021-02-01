using System;
using System.Collections.Generic;
using System.Text;

namespace Ui.Modules.ModuleName.Events
{
    public class SelectBomFinishEvent
    {
        public SelectBomFinishEvent(bool wasManuallyClosed)
        {
            WasManuallyClosed = wasManuallyClosed;
        }

        public bool WasManuallyClosed { get; set; }
    }
}
