using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.Svf.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ProMik.SmartIct.Console.ProjectFileHandler.Helper
{
    public class ProjectFileContent
    {
        public ProjectFileContent(
            byte[] odbProjectZipFolder,
            byte[] jsonProjectFile,
            byte[] settingsFile,
            byte[] bomFile,
            byte[] bomSettingsFile,
            SettingsContent settingsContent,
            BomSettings bomSettingsContent,
            Dictionary<string, byte[]> bsdlFiles,
            Dictionary<string, Stream> bsdlContent,
            Manifest manifest)
        {
            OdbProjectZipFolder = odbProjectZipFolder;
            JsonProjectFile = jsonProjectFile;
            SettingsFile = settingsFile;
            BomFile = bomFile;
            BomSettingsFile = bomSettingsFile;
            SettingsContent = settingsContent;
            BomSettingsContent = bomSettingsContent;
            BsdlFiles = bsdlFiles;
            BsdlContent = bsdlContent;
            Manifest = manifest;
        }

        public byte[] OdbProjectZipFolder { get; }

        public byte[] JsonProjectFile { get; }

        public byte[] SettingsFile { get; }

        public SettingsContent SettingsContent { get; }

        public byte[] BomFile { get; }

        public byte[] BomSettingsFile { get; }

        public BomSettings BomSettingsContent { get; }

        public Dictionary<string, Stream> BsdlContent { get; }

        public Dictionary<string, byte[]> BsdlFiles { get; }

        public Manifest Manifest { get; }
    }
}
