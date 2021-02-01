using System;
using System.Collections.Generic;
using System.Text;

namespace Ui.Modules.ModuleName.Events
{
    public class CloseGeneralSettingsEvent
    {
        public CloseGeneralSettingsEvent(bool closedManually)
        {
            WasClosedManually = closedManually;
        }

        public bool WasClosedManually { get; }
    }
}
