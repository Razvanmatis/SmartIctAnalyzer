using System.Collections.Generic;
using ProMik.Core.Interfaces.Events;

namespace TestCoverage.Events
{
    public class TestCoverageForDeterminingObjectsPerformedEvent : EventPayload
    {
        private readonly IList<string> objects;

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
