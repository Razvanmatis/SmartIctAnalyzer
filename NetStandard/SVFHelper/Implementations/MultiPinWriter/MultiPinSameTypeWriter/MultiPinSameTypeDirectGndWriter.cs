using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameTypeWriter
{
    public class MultiPinSameTypeDirectGndWriter : MultiPinWriterSameTypeBase
    {
        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.DirectGnd; }

        public override string DestinationPath { get => "directGnd" + NEIGHBOURS; }

        public override string Description { get => "directly connected to GND of same boundary scan test type"; }

        public override List<(bool isInput, bool expectedValue)> SvfCreationContent
        {
            get => new List<(bool isInput, bool expectedValue)>()
                {
                    (true, false),
                    (false, false)
                };
        }
    }
}
