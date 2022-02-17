using ProMik.SmartIct.Interfaces.Helper;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameNetWriter
{
    public class MultiPinSameNetPullUpPullDownWriter : MultiPinSameNetWriterBase
    {
        public MultiPinSameNetPullUpPullDownWriter(List<PinTestTypeContainer> pinsToUse) : base(pinsToUse)
        {
        }

        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.PullupPulldown; }

        public override string DestinationPath { get => "pullUpsAndDowns" + NEIGHBOURS; }

        public override string Description { get => "PULL UPS and PULL DOWNS with same nets"; }

        public override List<(bool isInput, bool expectedValue)> SvfCreationContent
        {
            get => new List<(bool isInput, bool expectedValue)>()
                {
                    (true, false),
                    (false, true),
                    (true, true),
                    (false, false)
                };
        }
    }
}
