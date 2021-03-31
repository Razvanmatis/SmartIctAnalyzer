using System.Collections.Generic;
using ProMik.Core.Interfaces.Events;

namespace Ui.Modules.ModuleName.Events
{
    public class SendLayersEvent : EventPayload
    {
        public SendLayersEvent(List<string> layers)
        {
            Layers = layers;
        }

        public List<string> Layers { get; }
    }
}
