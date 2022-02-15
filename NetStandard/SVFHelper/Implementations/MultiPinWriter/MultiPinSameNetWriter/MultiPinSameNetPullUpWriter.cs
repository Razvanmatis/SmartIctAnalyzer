using ProMik.SmartIct.Interfaces.Helper;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameNetWriter
{
    public class MultiPinSameNetPullUpWriter : MultiPinSameNetWriterBase
    {
        public MultiPinSameNetPullUpWriter(List<PinTestTypeContainer> pinsToUse) : base(pinsToUse)
        {
        }

        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.Pullup; }

        public override string DestinationPath { get => "pullUps" + NEIGHBOURS; }

        public override string Description { get => "PULL UPS with same nets"; }

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
