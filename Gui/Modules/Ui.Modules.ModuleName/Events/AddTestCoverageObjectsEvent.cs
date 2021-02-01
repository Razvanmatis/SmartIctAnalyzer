using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Interfaces.PcbInvestigator;
using Interfaces.TestCoverage;

namespace Ui.Modules.ModuleName.Events
{
    public class AddTestCoverageObjectsEvent
    {
        private Dictionary<TestCoverageObject, IList<IPCBComponent>> mapObjects;

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
