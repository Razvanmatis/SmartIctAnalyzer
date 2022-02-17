using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameTypeWriter
{
    public class MultiPinSameTypePullUpPullDownWriter : MultiPinWriterSameTypeBase
    {
        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.PullupPulldown; }

        public override string DestinationPath { get => "pullUpsAndDowns" + NEIGHBOURS; }

        public override string Description { get => "PULL UPS and PULL DOWNS of same boundary scan test type"; }

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
