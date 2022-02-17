using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.SinglePinWriter
{
    public class SinglePinDirectGndWriter : SinglePinPullDownWriter
    {
        public SinglePinDirectGndWriter(ILogger logger) : base(logger)
        {
        }

        public override string Description => "directly connected to GND";

        public override string DestinationPath => "directGnd";

        public override BoundaryScanTestType BasedTestType => BoundaryScanTestType.DirectGnd;

        public override List<(bool isInput, bool expectedValue)> SvfCreationContent =>
            new List<(bool isInput, bool expectedValue)>()
                {
                    (true, false),
                    (false, false)
                };

        public override BoundaryScanTestTypeInternal BoundaryScanTestTypeInternal => BoundaryScanTestTypeInternal.DirectGnd;
    }
}
