namespace TestCoverage.Helper
{
    public class IdentifierBlacklistContainer
    {
        public IdentifierBlacklistContainer(
            string gndNetIdentifier,
            string gndNetBlacklist,
            string powerNetIdentifier,
            string powerNetBlacklist,
            string jtagNetIdentifier,
            string jtagNetBlacklist)
        {
            GndNetIdentifier = gndNetIdentifier;
            GndNetBlacklist = gndNetBlacklist;
            PowerNetIdentifier = powerNetIdentifier;
            PowerNetBlacklist = powerNetBlacklist;
            JTAGNetIdentifier = jtagNetIdentifier;
            JTAGNetBlacklist = jtagNetBlacklist;
        }

        public string GndNetIdentifier { get; }

        public string GndNetBlacklist { get; }

        public string PowerNetIdentifier { get; }

        public string PowerNetBlacklist { get; }

        public string JTAGNetIdentifier { get; }

        public string JTAGNetBlacklist { get; }
    }
}
