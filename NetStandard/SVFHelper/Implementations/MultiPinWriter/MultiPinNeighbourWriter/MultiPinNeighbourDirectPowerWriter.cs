using ProMik.Core.Interfaces.Bsdl;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinNeighbourWriter
{
    public class MultiPinNeighbourDirectPowerWriter : MultiPinWriterNeighbourBase
    {
        public MultiPinNeighbourDirectPowerWriter(IBSDLPackage pinPackage, IBSDLOutput bsdlOutput)
            : base(pinPackage, bsdlOutput)
        {
        }

        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.DirectPower; }

        public override string DestinationPath { get => "directPower" + NEIGHBOURS; }

        public override string Description { get => "directly connected to Power with neighbours"; }
    }
}
