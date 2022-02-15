using ProMik.Core.Interfaces.Bsdl;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinNeighbourWriter
{
    public class MultiPinNeighbourPullUpWriter : MultiPinWriterNeighbourBase
    {
        public MultiPinNeighbourPullUpWriter(IBSDLPackage pinPackage, IBSDLOutput bsdlOutput)
            : base(pinPackage, bsdlOutput)
        {
        }

        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.Pullup; }

        public override string DestinationPath { get => "pullUps" + NEIGHBOURS; }

        public override string Description { get => "PULL UPS with neighbours"; }
    }
}
