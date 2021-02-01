using System.Collections.Generic;

namespace Ui.Modules.ModuleName.Events
{
    public class TestCoverageForDeterminingObjectsPerformedEvent
    {
        private IList<string> objects;

        public TestCoverageForDeterminingObjectsPerformedEvent(IList<string> objects)
        {
            this.objects = objects;
        }

        public IList<string> GetObjects()
        {
            return objects;
        }
    }
}
