using System.Collections.Generic;
using System.Threading.Tasks;
using ProMik.SmartIct.Interfaces.GrpcClientParser;

namespace ProMik.SmartIct.PCBComponentParser.Interfaces
{
    public interface IPCBComponentParser
    {
        Task<IParsedResult> GetParsedObjectsFromGrpc(
            string pathToOdb,
            string steps,
            IList<string> rIdentifier = null,
            IList<string> cIdentifier = null,
            IList<string> iIdentifier = null,
            IList<string> tpIdentifier = null,
            IList<string> icIdentifier = null,
            IList<string> conIdentifier = null,
            bool useContains = false);

        Task<IParsedResult> GetParsedObjectsFromGrpcByZipFolder(
            string pathToOdb,
            string steps,
            IList<string> rIdentifier = null,
            IList<string> cIdentifier = null,
            IList<string> iIdentifier = null,
            IList<string> tpIdentifier = null,
            IList<string> icIdentifier = null,
            IList<string> conIdentifier = null,
            bool useContains = false);

        Task<IParsedResult> GetParsedObjectsFromGrpcByZipFolder(
         byte[] odbContentAsZip,
         string steps,
         IList<string> rIdentifier = null,
         IList<string> cIdentifier = null,
         IList<string> iIdentifier = null,
         IList<string> tpIdentifier = null,
         IList<string> icIdentifier = null,
         IList<string> conIdentifier = null,
         bool useContains = false);

        string GetDataAsString(IParsedResult result);

        IParsedResult ImportComponentsFromFile(
            string data,
            IList<string> rIdentifier = null,
            IList<string> cIdentifier = null,
            IList<string> iIdentifier = null,
            IList<string> tpIdentifier = null,
            IList<string> icIdentifier = null,
            IList<string> conIdentifier = null,
            bool useContains = false);

        Task<bool> IsGrpcServerAvailable();

        Task<bool> ChangeIpAdressOfClient(string ip);
    }
}
