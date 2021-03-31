using ProMik.Core.Interfaces.Events;

namespace Ui.Modules.ModuleName.Events
{
    public class CloseAllSettingsEvent : EventPayload
    {
        public CloseAllSettingsEvent(bool manuallyClosed)
        {
            WasManuallyClosed = manuallyClosed;
        }

        public bool WasManuallyClosed { get; }
    }
}
