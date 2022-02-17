using ProMik.SmartIct.Interfaces.Helper;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameNetWriter
{
    public class MultiPinSameNetPullDownWriter : MultiPinSameNetWriterBase
    {
        public MultiPinSameNetPullDownWriter(List<PinTestTypeContainer> pinsToUse) : base(pinsToUse)
        {
        }

        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.Pulldown; }

        public override string DestinationPath { get => "pullDowns" + NEIGHBOURS; }

        public override string Description { get => "PULL DOWNS with same nets"; }

        public override List<(bool isInput, bool expectedValue)> SvfCreationContent
        {
            get => new List<(bool isInput, bool expectedValue)>()
                {
                    (true, false),
                    (false, true)
                };
        }
    }
}
