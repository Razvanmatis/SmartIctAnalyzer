using ProMik.Core.Interfaces.Results;
using ProMik.SmartIct.Console.Contracts.Container;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.GrpcClientParser;
using System.Threading.Tasks;

namespace ProMik.SmartIct.Console.OdbProjectExtractor.Interfaces
{
    public interface IOdbProjectExtractor
    {
        Task<Result<IParsedResult>> GetParsedObjectsFromGrpcByZipFolder(SettingsContent settings, string pathToOdb);

        Task<Result<IParsedResult>> GetParsedObjectsFromGrpcByZipFolder(SettingsContent settings, byte[] odbContentAsZip);

        Task<Result<string>> GetDataAsJsonString(IParsedResult result);

        Task<Result<IParsedResult>> GetParsedObjectsFromJsonFile(GeneralSettings settings, string jsonData);

        Task<Result<bool>> ChangeIpAdressOfClient(string ip);

        Task<Result<bool>> IsGrpcServerAvailable();

        Task<ProMik.Core.Interfaces.Results.Result> UpdateBom(IParsedResult data, string bomFilePath, BomSettings bomSettings);

        Task<ProMik.Core.Interfaces.Results.Result> UpdateBom(IParsedResult data, byte[] bomFileContent, BomSettings bomSettings);
    }
}
