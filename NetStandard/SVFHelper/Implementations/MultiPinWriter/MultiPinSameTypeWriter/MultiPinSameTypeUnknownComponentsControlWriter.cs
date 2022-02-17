using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinSameTypeWriter
{
    public class MultiPinSameTypeUnknownComponentsControlWriter : MultiPinWriterSameTypeBase
    {
        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.UnknownControl; }

        public override string DestinationPath { get => "unknownsControl" + NEIGHBOURS; }

        public override string Description { get => "UNKNOWN CONTROLS of same boundary scan test type"; }

        public override List<(bool isInput, bool expectedValue)> SvfCreationContent
        {
            get => new List<(bool isInput, bool expectedValue)>()
                {
                    (false, true),
                    (false, false)
                };
        }
    }
}
