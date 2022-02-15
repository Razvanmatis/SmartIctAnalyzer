using ProMik.SmartIct.Interfaces.Helper;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameNetWriter
{
    public class MultiPinSameNetDirectGndWriter : MultiPinSameNetWriterBase
    {
        public MultiPinSameNetDirectGndWriter(List<PinTestTypeContainer> pinsToUse) : base(pinsToUse)
        {
        }

        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.DirectGnd; }

        public override string DestinationPath { get => "directGnd" + NEIGHBOURS; }

        public override string Description { get => "directly connected to GND with same nets"; }

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
