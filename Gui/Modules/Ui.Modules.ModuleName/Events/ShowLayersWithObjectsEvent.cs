using System;
using System.Collections.Generic;
using System.Text;

namespace Ui.Modules.ModuleName.Events
{
    public class ShowLayersWithObjectsEvent
    {
        public ShowLayersWithObjectsEvent(IList<string> layersToShow)
        {
            LayersToShow = layersToShow;
        }

        public IList<string> LayersToShow { get; }
    }
}
