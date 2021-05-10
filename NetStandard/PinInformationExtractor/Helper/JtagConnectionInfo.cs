using System.Collections.Generic;

namespace PinInformationExtractor.Helper
{
    public class JtagConnectionInfo
    {
        public JtagConnectionInfo(
            string componentName, 
            List<PinConnectionInfo> pins, 
            int totalAmountOfNets, 
            int totalAmountOfUnknownComponents,
            int totalAmountOfPullUpComponents,
            int totalAmountOfPullDownComponents,
            int totalAmountOfNotConnectedComponents,
            int totalAmountOfToIcComponents,
            int totalAmountOfToJTAGComponents,
            int totalAmountOfJTAGComponents)
        {
            ComponentName = componentName;
            Pins = pins;
            TotalAmountOfPins = pins.Count;
            TotalAmountOfNets = totalAmountOfNets;
            TotalAmountOfUnknownComponents = totalAmountOfUnknownComponents;
            TotalAmountOfPullUpComponents = totalAmountOfPullUpComponents;
            TotalAmountOfPullDownComponents = totalAmountOfPullDownComponents;
            TotalAmountOfNotConnectedComponents = totalAmountOfNotConnectedComponents;
            TotalAmountOfToIcComponents = totalAmountOfToIcComponents;
            TotalAmountOfToJTAGComponents = totalAmountOfToJTAGComponents;
            TotalAmountOfJTAGComponents = totalAmountOfJTAGComponents;
        }

        public string ComponentName { get; set; }

        public int TotalAmountOfPins { get; set; }

        public int TotalAmountOfNets { get; set; }

        public int TotalAmountOfUnknownComponents { get; set; }

        public int TotalAmountOfPullUpComponents { get; set; }

        public int TotalAmountOfPullDownComponents { get; set; }

        public int TotalAmountOfNotConnectedComponents { get; set; }

        public int TotalAmountOfToIcComponents { get; set; }

        public int TotalAmountOfToJTAGComponents { get; set; }

        public int TotalAmountOfJTAGComponents { get; set; }

        public List<PinConnectionInfo> Pins { get; set; }
    }
}
