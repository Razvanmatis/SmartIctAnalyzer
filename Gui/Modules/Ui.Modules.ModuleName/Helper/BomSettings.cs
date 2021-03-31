namespace Ui.Modules.ModuleName.Helper
{
    public class BomSettings
    {
        public BomSettings(string separator, string colRef, string colValue)
        {
            Separator = separator;
            ColRef = colRef;
            ColValue = colValue;
        }

        public string Separator { get; set; }

        public string ColRef { get; set; }

        public string ColValue { get; set; }
    }
}
