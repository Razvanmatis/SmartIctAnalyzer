using System;
using System.Collections.Generic;
using System.Text;

namespace Ui.Modules.ModuleName.Events
{
    public class CloseTestCoverageSettingsEvent
    {
        public CloseTestCoverageSettingsEvent(bool closedManually)
        {
            WasClosedManually = closedManually;
        }

        public bool WasClosedManually { get; }
    }
}
