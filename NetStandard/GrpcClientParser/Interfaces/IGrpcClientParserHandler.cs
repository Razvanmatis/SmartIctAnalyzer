using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Interfaces.PcbInvestigator;

namespace GrpcClientParser.Interfaces
{
    public interface IGrpcClientParserHandler
    {
        Task<IParsedResult> GetParsedObjectsFromGrpc(
            string pathToOdb,
            IList<string> rIdentifier = null,
            IList<string> cIdentifier = null,
            IList<string> iIdentifier = null,
            IList<string> tpIdentifier = null,
            IList<string> icIdentifier = null,
            IList<string> conIdentifier = null);

        Task<IParsedResult> GetParsedObjectsFromGrpcByZipFolder(
            string pathToOdb,
            IList<string> rIdentifier = null,
            IList<string> cIdentifier = null,
            IList<string> iIdentifier = null,
            IList<string> tpIdentifier = null,
            IList<string> icIdentifier = null,
            IList<string> conIdentifier = null);

        void ExportComponentsToFile(IParsedResult result, string filePath);

        IParsedResult ImportComponentsFromFile(
            string filePath,
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
