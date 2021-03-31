using ProMik.Core.Interfaces.Events;

namespace Ui.Modules.ModuleName.Events
{
    public class SetBusyEvent : EventPayload
    {
        public SetBusyEvent(bool isBusy)
        {
            IsBusy = isBusy;
        }

        public bool IsBusy { get; }
    }
}
