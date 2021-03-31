using System.Collections.Generic;
using Interfaces.PcbInvestigator;
using ProMik.Core.Interfaces.Events;
using TestCoverage.Interfaces;

namespace TestCoverage.Events
{
    public class AddTestCoverageObjectsEvent : EventPayload
    {
        private readonly Dictionary<TestCoverageObject, IList<IPCBComponent>> mapObjects;

        public AddTestCoverageObjectsEvent(Dictionary<TestCoverageObject, IList<IPCBComponent>> mapObjects)
        {
            this.mapObjects = mapObjects;
        }

        public Dictionary<TestCoverageObject, IList<IPCBComponent>> GetObjects()
        {
            return mapObjects;
        }
    }
}
