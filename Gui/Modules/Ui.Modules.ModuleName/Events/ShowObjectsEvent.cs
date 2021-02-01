using System;
using System.Collections.Generic;
using System.Text;

namespace Ui.Modules.ModuleName.Events
{
    public class ShowObjectsEvent
    {
        private IList<string> objects;

        public ShowObjectsEvent(IList<string> objects)
        {
            this.objects = objects;
        }

        public IList<string> GetObjects()
        {
            return objects;
        }
    }
}
