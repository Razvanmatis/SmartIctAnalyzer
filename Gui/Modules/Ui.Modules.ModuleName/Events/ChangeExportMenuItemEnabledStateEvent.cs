using System;
using System.Collections.Generic;
using System.Text;

namespace Ui.Modules.ModuleName.Events
{
    public class ChangeExportMenuItemEnabledStateEvent
    {
        public ChangeExportMenuItemEnabledStateEvent(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }

        public bool IsEnabled { get; private set; }
    }
}
