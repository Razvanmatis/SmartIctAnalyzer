using ProMik.Core.Interfaces.Events;

namespace Ui.Modules.ModuleName.Events
{
    public class ChangeExportMenuItemEnabledStateEvent : EventPayload
    {
        public ChangeExportMenuItemEnabledStateEvent(bool isEnabled)
        {
            IsEnabled = isEnabled;
        }

        public bool IsEnabled { get; private set; }
    }
}
