using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameTypeWriter
{
    public class MultiPinSameTypeDirectPowerWriter : MultiPinWriterSameTypeBase
    {
        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.DirectPower; }

        public override string DestinationPath { get => "directPower" + NEIGHBOURS; }

        public override string Description { get => "directly connected to Power of same boundary scan test type"; }

        public override List<(bool isInput, bool expectedValue)> SvfCreationContent
        {
            get => new List<(bool isInput, bool expectedValue)>()
                {
                    (true, true),
                    (false, true)
                };
        }
    }
}
