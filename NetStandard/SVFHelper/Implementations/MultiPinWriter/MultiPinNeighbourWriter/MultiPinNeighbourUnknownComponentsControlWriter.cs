using ProMik.Core.Interfaces.Bsdl;
using ProMik.Svf.Contracts;
using ProMik.SvfPlayer_sharp;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations.MultiPinWriter.MultiPinNeighbourWriter
{
    public class MultiPinNeighbourUnknownComponentsControlWriter : MultiPinWriterNeighbourBase
    {
        public MultiPinNeighbourUnknownComponentsControlWriter(IBSDLPackage pinPackage, IBSDLOutput bsdlOutput)
            : base(pinPackage, bsdlOutput)
        {
        }

        public override BoundaryScanTestType BasedTestType { get => BoundaryScanTestType.UnknownControl; }

        public override string DestinationPath { get => "unknownsControl" + NEIGHBOURS; }

        public override string Description { get => "UNKNOWN CONTROLS with neighbours"; }
    }
}
