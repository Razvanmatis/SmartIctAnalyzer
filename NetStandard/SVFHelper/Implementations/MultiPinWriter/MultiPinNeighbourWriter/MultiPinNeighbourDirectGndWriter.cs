using ProMik.Core.Interfaces.Bsdl;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinNeighbourWriter
{
    public class MultiPinNeighbourDirectGndWriter : MultiPinWriterNeighbourBase
    {
        public MultiPinNeighbourDirectGndWriter(IBSDLPackage pinPackage, IBSDLOutput bsdlOutput)
            : base(pinPackage, bsdlOutput)
        {
        }

        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.DirectGnd; }

        public override string DestinationPath { get => "directGnd" + NEIGHBOURS; }

        public override string Description { get => "directly connected to GND with neighbours"; }
    }
}
