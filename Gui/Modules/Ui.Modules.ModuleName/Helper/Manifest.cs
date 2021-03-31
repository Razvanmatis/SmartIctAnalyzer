namespace Ui.Modules.ModuleName.Helper
{
    public class Manifest
    {
        public const string MANIFESTNAME = "project.manifest";
        public const string ODBPROJECTDEF = "odbProject\\";
        public const string JSONLOCDEF = "jsonProject\\";
        public const string SVFDEF = "svf\\";
        public const string JSONPROJECTDEF = JSONLOCDEF + "jsonProject.json";
        public const string SETTINGSLOCDEF = "settings\\";
        public const string SETTINGSDEF = SETTINGSLOCDEF + "settings.db";
        public const string BOMLOCDEF = "bom\\";
        public const string BOMFILEDEF = BOMLOCDEF + "bomFile.csv";
        public const string BOMSETTINGSDEF = BOMLOCDEF + "bomSettings.json";
        public const string VERSIONDEF = "1.0";

        public Manifest()
        {
            OdbProject = ODBPROJECTDEF;
            JsonProject = JSONPROJECTDEF;
            SettingsFile = SETTINGSDEF;
            BomFile = BOMFILEDEF;
            BomSettingsFile = BOMSETTINGSDEF;
            Version = VERSIONDEF;
            Svf = SVFDEF;
        }

        public string Version { get; set; }

        public string OdbProject { get; set; }

        public string JsonProject { get; set; }

        public string SettingsFile { get; set; }

        public string BomFile { get; set; }

        public string Svf { get; set; }

        public string BomSettingsFile { get; set; }

        public void SetJsonProject(string value)
        {
            JsonProject = JSONLOCDEF + value;
        }

        public void SetSettingsFile(string value)
        {
            SettingsFile = SETTINGSLOCDEF + value;
        }

        public void SetBomFile(string value)
        {
            BomFile = BOMLOCDEF + value;
        }

        public void SetBomSettingsFile(string value)
        {
            BomSettingsFile = BOMLOCDEF + value;
        }
    }
}
