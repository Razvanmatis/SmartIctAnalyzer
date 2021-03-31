using ProMik.Core.Interfaces.Events;

namespace Ui.Modules.ModuleName.Events
{
    public class HandleDropEvent : EventPayload
    {
        public HandleDropEvent(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; }
    }
}
