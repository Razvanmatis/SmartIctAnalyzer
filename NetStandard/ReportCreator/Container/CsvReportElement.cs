namespace ProMik.SmartIct.Services.ReportCreator.Container
{
    internal class CsvReportElement
    {
        public CsvReportElement(
            string jtag,
            string svfType,
            string device,
            string pinNumber,
            string deviceType,
            string netName,
            string controlState,
            string expectedState,
            string test)
        {
            JTAG = jtag;
            SvfType = svfType;
            Device = device;
            PinNumber = pinNumber;
            DeviceType = deviceType;
            NetName = netName;
            ControlState = controlState;
            ExpectedState = expectedState;
            Test = test;
        }

        public string JTAG { get; set; }

        public string SvfType { get; set; }

        public string Device { get; set; }

        public string PinNumber { get; set; }

        public string DeviceType { get; set; }

        public string NetName { get; set; }

        public string ControlState { get; set; }

        public string ExpectedState { get; set; }

        public string Test { get; set; }
    }
}
