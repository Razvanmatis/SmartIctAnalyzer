using ProMik.SmartIct.Interfaces.Helper;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameNetWriter
{
    public class MultiPinSameNetUnknownComponentsInputWriter : MultiPinSameNetWriterBase
    {
        public MultiPinSameNetUnknownComponentsInputWriter(List<PinTestTypeContainer> pinsToUse) : base(pinsToUse)
        {
        }

        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.UnknownInput; }

        public override string DestinationPath { get => "unknownsInput" + NEIGHBOURS; }

        public override string Description { get => "UNKNOWN INPUTS with same nets"; }

        public override List<(bool isInput, bool expectedValue)> SvfCreationContent
        {
            get => new List<(bool isInput, bool expectedValue)>()
                {
                    (true, true),
                    (true, false)
                };
        }
    }
}
