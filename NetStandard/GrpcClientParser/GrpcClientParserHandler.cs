using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using GrpcClientParser.Helper.JsonObjects;
using GrpcClientParser.Interfaces;
using Interfaces.PcbInvestigator;
using static PcbInvestigatorService;

namespace GrpcClientParser
{
    public class GrpcClientParserHandler : IGrpcClientParserHandler
    {
        private const int PORT = 50151;
        private const string DEFIP = "127.0.0.1";
        private static readonly string FILEPATH = Directory.GetCurrentDirectory() + "\\grpcRequest.zip";
        private static readonly Func<IFunctionalAttributes, bool> ISTESTPOINTDEF = new Func<IFunctionalAttributes, bool>(x =>
            x.PartName.Contains("testpoint") || x.PackageName.Contains("TP") || x.PartName.Contains("TP"));

        private static PcbInvestigatorServiceClient client;
        private GrpcChannel channel;

        public GrpcClientParserHandler()
        {
            SetSwitch(DEFIP);
        }

        public async Task<bool> IsGrpcServerAvailable()
        {
            try
            {
                if (client == null)
                {
                    return false;
                }

                StatusResult result = await client.ClientAvailableAsync(new Empty(), new CallOptions(deadline: DateTime.UtcNow.AddSeconds(10)));
                if (!result.Status)
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("Serve is not available!");
                return false;
            }

            return true;
        }

        public async Task<bool> ChangeIpAdressOfClient(string ip)
        {
            SetSwitch(ip);
            return await IsGrpcServerAvailable().ConfigureAwait(true);
        }

        public async Task<IParsedResult> GetParsedObjectsFromGrpc(
            string pathToOdb,
            IList<string> rIdentifier = null,
            IList<string> cIdentifier = null,
            IList<string> iIdentifier = null,
            IList<string> tpIdentifier = null,
            IList<string> icIdentifier = null,
            IList<string> conIdentifier = null)
        {
            Request request = new Request();
            request.PathToOdb = pathToOdb;
            Result result = await client.GetConvertedObjectsAsync(request);
            return FinalizeHandling(result, rIdentifier, cIdentifier, iIdentifier, tpIdentifier, icIdentifier, conIdentifier);
        }

        public void ExportComponentsToFile(IParsedResult result, string filePath)
        {
            JsonImportExportHelper.ExportComponentsToFile(result, filePath);
        }

        public IParsedResult ImportComponentsFromFile(
            string filePath,
            IList<string> rIdentifier = null,
            IList<string> cIdentifier = null,
            IList<string> iIdentifier = null,
            IList<string> tpIdentifier = null,
            IList<string> icIdentifier = null,
            IList<string> conIdentifier = null)
        {
            Func<IFunctionalAttributes, bool> isTestPoint = ISTESTPOINTDEF;
            if (tpIdentifier != null)
            {
                isTestPoint = GetTpFunction(tpIdentifier);
            }

            return JsonImportExportHelper.ImportComponentsFromFile(
                filePath,
                rIdentifier,
                cIdentifier,
                iIdentifier,
                icIdentifier,
                conIdentifier,
                isTestPoint);
        }

        public async Task<IParsedResult> GetParsedObjectsFromGrpcByZipFolder(string pathToOdb, IList<string> rIdentifier = null, IList<string> cIdentifier = null, IList<string> iIdentifier = null, IList<string> tpIdentifier = null, IList<string> icIdentifier = null, IList<string> conIdentifier = null)
        {
            RequestZip request = new RequestZip();
            byte[] data = GetZipFolderAsBytes(pathToOdb);
            if (data == null || data.Length == 0)
            {
                return null;
            }

            try
            {
                File.Delete(FILEPATH);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error deleting zip file!" + e.Message);
            }

            request.ZipFolder = ByteString.CopyFrom(data);
            Result result = await client.GetConvertedObjectsByZipAsync(request);
            return FinalizeHandling(result, rIdentifier, cIdentifier, iIdentifier, tpIdentifier, icIdentifier, conIdentifier);
        }

        private static int GetTotalPinAmount(IList<IPCBComponent> components)
        {
            List<IPinComponent> pins = new List<IPinComponent>();
            foreach (var comp in components)
            {
                foreach (var pin in comp.Connections)
                {
                    if (!pins.Contains(pin))
                    {
                        pins.Add(pin);
                    }
                }
            }

            return pins.Count;
        }

        private static Func<IFunctionalAttributes, bool> GetTpFunction(IList<string> tpIdentifier)
        {
            return new Func<IFunctionalAttributes, bool>(x =>
            {
                foreach (var text in tpIdentifier)
                {
                    if (x.PartName.Contains(text) || x.PackageName.Contains(text))
                    {
                        return true;
                    }
                }

                return false;
            });
        }

        private static IParsedResult FinalizeHandling(Result result, IList<string> rIdentifier, IList<string> cIdentifier, IList<string> iIdentifier, IList<string> tpIdentifier, IList<string> icIdentifier, IList<string> conIdentifier)
        {
            string message = "Received from GRPC amount PCB Components: " + result.Components.Count + ", nets: " + result.Nets.Count + " and pins: " + result.Pins.Count;
            Debug.WriteLine(message);
            Console.WriteLine(message);
            IResultJson resultJson = GrpcJsonConverter.GetResultJson(result);

            Func<IFunctionalAttributes, bool> isTestPoint = ISTESTPOINTDEF;
            if (tpIdentifier != null)
            {
                isTestPoint = GetTpFunction(tpIdentifier);
            }

            IList<IPCBComponent> components = JsonImportExportHelper.GetParsedComponents(
                resultJson,
                rIdentifier,
                cIdentifier,
                iIdentifier,
                icIdentifier,
                conIdentifier,
                isTestPoint);
            IList<INetComponent> nets = JsonImportExportHelper.GetParsedNets(components);

            int pinAmount = GetTotalPinAmount(components);
            message = "After parsing creation of total objects PCB Components: " + components.Count + ", nets: " + nets.Count + " and pins: " + pinAmount;
            Debug.WriteLine(message);
            Console.WriteLine(message);
            return new ParsedResult(components, nets);
        }

        private static byte[] GetZipFolderAsBytes(string pathToOdb)
        {
            if (!Directory.Exists(pathToOdb))
            {
                Debug.WriteLine("Folder does not exist!" + pathToOdb);
                return null;
            }

            try
            {
                ZipFile.CreateFromDirectory(pathToOdb, FILEPATH);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }

            if (!File.Exists(FILEPATH))
            {
                Debug.WriteLine("Zip file does not exist!" + FILEPATH);
                return null;
            }

            return File.ReadAllBytes(FILEPATH);
        }

        private void SetSwitch(string ip)
        {
            AppContext.SetSwitch(
                "System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = GrpcChannel.ForAddress("http://" + ip + ":" + PORT);
            client = new PcbInvestigatorService.PcbInvestigatorServiceClient(channel);
        }
    }
}
