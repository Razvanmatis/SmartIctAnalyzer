using System.Collections.Generic;
using ProMik.Core.Interfaces.Events;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.TestCoverageDeterminer.Interfaces;

namespace ProMik.SmartIct.TestCoverageDeterminer.Events
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
