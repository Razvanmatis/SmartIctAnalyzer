using System.Collections.Generic;
using ProMik.Core.Interfaces.Events;

namespace Ui.Modules.ModuleName.Events
{
    public class ShowObjectsEvent : EventPayload
    {
        private readonly IList<string> objects;

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
