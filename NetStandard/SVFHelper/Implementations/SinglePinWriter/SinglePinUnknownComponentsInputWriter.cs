using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.SinglePinWriter
{
    public class SinglePinUnknownComponentsInputWriter : SinglePinUnknownComponentsControlWriter
    {
        public SinglePinUnknownComponentsInputWriter(ILogger logger) : base(logger)
        {
        }

        public override string Description => "UNKNOWN INPUTS";

        public override string DestinationPath => "unknownsInput";

        public override BoundaryScanTestTypeInternal BoundaryScanTestTypeInternal => BoundaryScanTestTypeInternal.UnknownInput;

        public override BoundaryScanTestType BasedTestType => BoundaryScanTestType.UnknownInput;

        public override List<(bool isInput, bool expectedValue)> SvfCreationContent =>
            new List<(bool isInput, bool expectedValue)>()
                {
                    (true, true),
                    (true, false)
                };
    }
}
