using Ionic.Zip;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.Svf.Contracts;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ProMik.SmartIct.Services.ManifestHandler.Interfaces
{
    public interface IManifestHandling
    {
        Manifest Manifest { get; }

        string ManifestProjectFilePath { get; }

        ZipFile GetArchiveForPath(string path);

        void DeleteContentFromZipWithAllSubfolders(string nameToUse);

        T GetObjectFromZipArchive<T>(string fileName);

        void SetPathAndManifestOfZipfile(string path, bool newCreation);

        string GetObjecttAsJsonString(object obj);

        void ResetValues(bool skipFolderDeletion = false);

        bool UpdateBomFile(byte[] fileContent, string newFileName);

        bool UpdateBomSettingsFile(BomSettings fileContent, string newFileName);

        bool UpdateManifestInZipfile(Manifest manifest);

        Task<bool> UpdateOdbProject(string pathToFolder);

        bool SaveBsdlContentIntoProjectFile(BsdlContainer bsdl, byte[] data);
    }
}
