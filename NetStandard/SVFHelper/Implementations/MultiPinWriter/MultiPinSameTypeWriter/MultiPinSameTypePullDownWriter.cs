using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameTypeWriter
{
    public class MultiPinSameTypePullDownWriter : MultiPinWriterSameTypeBase
    {
        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.Pulldown; }

        public override string DestinationPath { get => "pullDowns" + NEIGHBOURS; }

        public override string Description { get => "PULL DOWNS of same boundary scan test type"; }

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
