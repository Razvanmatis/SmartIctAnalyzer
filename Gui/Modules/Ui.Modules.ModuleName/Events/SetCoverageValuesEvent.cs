using ProMik.Core.Interfaces.Events;

namespace Ui.Modules.ModuleName.Events
{
    public class SetCoverageValuesEvent : EventPayload
    {
        public SetCoverageValuesEvent(float value, bool isVisible)
        {
            Value = value;
            IsVisible = isVisible;
        }

        public float Value { get; set; }

        public bool IsVisible { get; set; }
    }
}
