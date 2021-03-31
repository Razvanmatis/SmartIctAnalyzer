using ProMik.Core.Interfaces.Events;

namespace Ui.Modules.ModuleName.Events
{
    public class SelectBomFinishEvent : EventPayload
    {
        public SelectBomFinishEvent(bool wasManuallyClosed, bool autoMode)
        {
            WasManuallyClosed = wasManuallyClosed;
            AutoModeEnabled = autoMode;
        }

        public bool WasManuallyClosed { get; set; }

        public bool AutoModeEnabled { get; set; }
    }
}
