using System.Collections.Generic;
using System.Threading.Tasks;

namespace GrpcClientParser.Interfaces
{
    public interface IGrpcClientParserHandler
    {
        Task<IParsedResult> GetParsedObjectsFromGrpc(
            string pathToOdb,
            string steps,
            IList<string> rIdentifier = null,
            IList<string> cIdentifier = null,
            IList<string> iIdentifier = null,
            IList<string> tpIdentifier = null,
            IList<string> icIdentifier = null,
            IList<string> conIdentifier = null);

        Task<IParsedResult> GetParsedObjectsFromGrpcByZipFolder(
            string pathToOdb,
            string steps,
            IList<string> rIdentifier = null,
            IList<string> cIdentifier = null,
            IList<string> iIdentifier = null,
            IList<string> tpIdentifier = null,
            IList<string> icIdentifier = null,
            IList<string> conIdentifier = null);

        string GetDataAsString(IParsedResult result);

        IParsedResult ImportComponentsFromFile(
            string data,
            IList<string> rIdentifier = null,
            IList<string> cIdentifier = null,
            IList<string> iIdentifier = null,
            IList<string> tpIdentifier = null,
            IList<string> icIdentifier = null,
            IList<string> conIdentifier = null);

        Task<bool> IsGrpcServerAvailable();

        Task<bool> ChangeIpAdressOfClient(string ip);
    }
}
