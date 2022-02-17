using System.Collections.Generic;
using System.Linq;
using ProMik.SmartIct.Interfaces.Helper;

namespace ProMik.SmartIct.Interfaces.Container
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
            int totalAmountOfJTAGComponents,
            int totalAmountOfGndComponents,
            int totalAmountOfPowerComponents)
        {
            ComponentName = componentName.Trim();
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
            TotalAmountOfGndComponents = totalAmountOfGndComponents;
            TotalAmountOfPowerComponents = totalAmountOfPowerComponents;
        }

        public string ComponentName { get; set; }

        public string ComponentRealName { get; set; }

        public int TotalAmountOfPins { get; set; }

        public int TotalAmountOfNets { get; set; }

        public int TotalAmountOfUnknownComponents { get; set; }

        public int TotalAmountOfPullUpComponents { get; set; }

        public int TotalAmountOfPullDownComponents { get; set; }

        public int TotalAmountOfNotConnectedComponents { get; set; }

        public int TotalAmountOfToIcComponents { get; set; }

        public int TotalAmountOfToJTAGComponents { get; set; }

        public int TotalAmountOfJTAGComponents { get; set; }

        public int TotalAmountOfGndComponents { get; set; }

        public int TotalAmountOfPowerComponents { get; set; }

        public List<PinConnectionInfo> Pins { get; set; }

        public List<PinTestTypeContainer> GetAllDirectGndPins()
        {
            return GetAllPinsOfType(PinConnectionType.GND);
        }

        public List<PinTestTypeContainer> GetAllDirectPowerPins()
        {
            return GetAllPinsOfType(PinConnectionType.POWER);
        }

        public List<PinTestTypeContainer> GetAllPullDownPins()
        {
            return GetAllPinsOfType(PinConnectionType.PULLDOWN);
        }

        public List<PinTestTypeContainer> GetAllPullUpPins()
        {
            return GetAllPinsOfType(PinConnectionType.PULLUP);
        }

        public List<PinTestTypeContainer> GetAllOtherPins()
        {
            return GetAllPinsOfType(PinConnectionType.OTHER);
        }

        private static bool ContainsAlreadyPin(string pinNumber, List<PinTestTypeContainer> names)
        {
            return names.FirstOrDefault(obj => obj.PinTestObject.PinNumber.Equals(pinNumber)) != null;
        }

        private List<PinTestTypeContainer> GetAllPinsOfType(PinConnectionType type)
        {
            List<PinTestTypeContainer> names = new List<PinTestTypeContainer>();
            foreach (var pin in Pins)
            {
                if (ContainsAlreadyPin(pin.PinNumber, names))
                {
                    continue;
                }

                foreach (var typeInner in pin.PinConnections)
                {
                    if (typeInner.PinConnectionType == type)
                    {
                        names.Add(new PinTestTypeContainer()
                        { PinTestObject = new PinTestObject(pin.PinNumber, pin.NetNames), PinConnectionType = typeInner });
                    }
                }
            }

            return names;
        }
    }
}
