using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameTypeWriter
{
    public class MultiPinSameTypePullUpWriter : MultiPinWriterSameTypeBase
    {
        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.Pullup; }

        public override string DestinationPath { get => "pullUps" + NEIGHBOURS; }

        public override string Description { get => "PULL UPS of same boundary scan test type"; }

        public override List<(bool isInput, bool expectedValue)> SvfCreationContent
        {
            get => new List<(bool isInput, bool expectedValue)>()
                {
                    (true, true),
                    (false, false)
                };
        }
    }
}
