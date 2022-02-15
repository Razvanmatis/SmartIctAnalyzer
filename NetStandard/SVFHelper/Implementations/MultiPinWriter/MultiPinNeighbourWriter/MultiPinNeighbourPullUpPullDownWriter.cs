using ProMik.Core.Interfaces.Bsdl;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinNeighbourWriter
{
    public class MultiPinNeighbourPullUpPullDownWriter : MultiPinWriterNeighbourBase
    {
        public MultiPinNeighbourPullUpPullDownWriter(IBSDLPackage pinPackage, IBSDLOutput bsdlOutput)
            : base(pinPackage, bsdlOutput)
        {
        }

        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.PullupPulldown; }

        public override string DestinationPath { get => "pullUpsAndDowns" + NEIGHBOURS; }

        public override string Description { get => "PULL UPS and PULL DOWNS with neighbours"; }
    }
}
