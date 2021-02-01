using System;
using System.Collections.Generic;
using System.Text;

namespace Ui.Modules.ModuleName.Events
{
    public class SendLayersEvent
    {
        public SendLayersEvent(List<string> layers)
        {
            Layers = layers;
        }

        public List<string> Layers { get; }
    }
}
