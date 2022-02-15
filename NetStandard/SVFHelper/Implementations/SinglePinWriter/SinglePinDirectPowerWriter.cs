using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.SinglePinWriter
{
    public class SinglePinDirectPowerWriter : SinglePinPullUpWriter
    {
        public SinglePinDirectPowerWriter(ILogger logger) : base(logger)
        {
        }

        public override string Description => "directly connected to POWER";

        public override string DestinationPath => "directPower";

        public override BoundaryScanTestType BasedTestType => BoundaryScanTestType.DirectPower;

        public override List<(bool isInput, bool expectedValue)> SvfCreationContent =>
            new List<(bool isInput, bool expectedValue)>()
                {
                    (true, true),
                    (false, true)
                };

        public override BoundaryScanTestTypeInternal BoundaryScanTestTypeInternal => BoundaryScanTestTypeInternal.DirectPower;
    }
}
