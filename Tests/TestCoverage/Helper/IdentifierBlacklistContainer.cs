namespace ProMik.SmartIct.TestCoverageDeterminer.Helper
{
    public class IdentifierBlacklistContainer
    {
        public IdentifierBlacklistContainer(
            string gndNetIdentifier,
            string gndNetBlacklist,
            string powerNetIdentifier,
            string powerNetBlacklist,
            string jTAGPinIdentifier,
            string jtagNetBlacklist)
        {
            GndNetIdentifier = gndNetIdentifier;
            GndNetBlacklist = gndNetBlacklist;
            PowerNetIdentifier = powerNetIdentifier;
            PowerNetBlacklist = powerNetBlacklist;
            JTAGNetBlacklist = jtagNetBlacklist;
            JTAGPinIdentifier = jTAGPinIdentifier;
        }

        public string GndNetIdentifier { get; }

        public string GndNetBlacklist { get; }

        public string PowerNetIdentifier { get; }

        public string PowerNetBlacklist { get; }

        public string JTAGNetBlacklist { get; }

        public string JTAGPinIdentifier { get; }
    }
}
