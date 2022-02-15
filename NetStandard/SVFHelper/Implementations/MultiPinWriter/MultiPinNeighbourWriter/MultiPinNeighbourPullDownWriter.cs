using ProMik.Core.Interfaces.Bsdl;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinNeighbourWriter
{
    public class MultiPinNeighbourPullDownWriter : MultiPinWriterNeighbourBase
    {
        public MultiPinNeighbourPullDownWriter(IBSDLPackage pinPackage, IBSDLOutput bsdlOutput)
            : base(pinPackage, bsdlOutput)
        {
        }

        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.Pulldown; }

        public override string DestinationPath { get => "pullDowns" + NEIGHBOURS; }

        public override string Description { get => "PULL DOWNS with neighbours"; }
    }
}
