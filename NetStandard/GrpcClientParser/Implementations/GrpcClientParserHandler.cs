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
using Interfaces.Gui;
using Interfaces.PcbInvestigator;
using static PcbInvestigatorService;

namespace GrpcClientParser.Implementations
{
    public class GrpcClientParserHandler : IGrpcClientParserHandler
    {
        private const int PORT = 50151;
        private const string DEFIP = "127.0.0.1";
        private static readonly string FILEPATH = Directory.GetCurrentDirectory() + "\\grpcRequest.zip";
        private static readonly Func<IFunctionalAttributes, bool> ISTESTPOINTDEF = new Func<IFunctionalAttributes, bool>(x =>
            x.PartName.Contains("testpoint") || x.PackageName.Contains("TP") || x.PartName.Contains("TP"));

        private static PcbInvestigatorServiceClient client;
        private readonly ILogger logger;
        private GrpcChannel channel;

        public GrpcClientParserHandler(ILogger logger)
        {
            this.logger = logger;
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
            catch (RpcException e)
            {
                logger.LogMessage("Serve is not available:" + e.Message, LogCategory.ERROR);
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
            string steps,
            IList<string> rIdentifier = null,
            IList<string> cIdentifier = null,
            IList<string> iIdentifier = null,
            IList<string> tpIdentifier = null,
            IList<string> icIdentifier = null,
            IList<string> conIdentifier = null)
        {
            Request request = new Request
            {
                PathToOdb = pathToOdb,
                Steps = steps,
            };
            Result result = await client.GetConvertedObjectsAsync(request);
            return FinalizeHandling(result, rIdentifier, cIdentifier, iIdentifier, tpIdentifier, icIdentifier, conIdentifier);
        }

        public string GetDataAsString(IParsedResult result)
        {
            return JsonImportExportHelper.GetDataAsString(result, logger);
        }

        public IParsedResult ImportComponentsFromFile(
            string data,
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

            Stopwatch sw = Stopwatch.StartNew();
            var result = JsonImportExportHelper.ImportComponentsFromFile(
                data,
                rIdentifier,
                cIdentifier,
                iIdentifier,
                icIdentifier,
                conIdentifier,
                isTestPoint,
                logger);
            sw.Stop();
            Debug.WriteLine("Stage IMPORT: {0}ms", sw.Elapsed.TotalMilliseconds);
            return result;
        }

        public async Task<IParsedResult> GetParsedObjectsFromGrpcByZipFolder(string pathToOdb, string steps, IList<string> rIdentifier = null, IList<string> cIdentifier = null, IList<string> iIdentifier = null, IList<string> tpIdentifier = null, IList<string> icIdentifier = null, IList<string> conIdentifier = null)
        {
            Stopwatch sw = Stopwatch.StartNew();
            byte[] data = GetZipFolderAsBytes(pathToOdb);
            sw.Stop();
            Debug.WriteLine("Compress all projectfiles into zip: {0}ms", sw.Elapsed.TotalMilliseconds);
            if (data == null || data.Length == 0)
            {
                return null;
            }

            try
            {
                File.Delete(FILEPATH);
            }
            catch (IOException e)
            {
                logger.LogMessage("Error deleting zip file!" + e.Message, LogCategory.ERROR);
            }
            catch (ArgumentException e)
            {
                logger.LogMessage("Error deleting zip file!" + e.Message, LogCategory.ERROR);
            }
            catch (NotSupportedException e)
            {
                logger.LogMessage("Error deleting zip file!" + e.Message, LogCategory.ERROR);
            }
            catch (UnauthorizedAccessException e)
            {
                logger.LogMessage("Error deleting zip file!" + e.Message, LogCategory.ERROR);
            }

            RequestZip request = new RequestZip()
            {
                ZipFolder = ByteString.CopyFrom(data),
                Steps = steps,
            };
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

        private IParsedResult FinalizeHandling(Result result, IList<string> rIdentifier, IList<string> cIdentifier, IList<string> iIdentifier, IList<string> tpIdentifier, IList<string> icIdentifier, IList<string> conIdentifier)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string message = "Received from GRPC amount PCB Components: " + result.Components.Count + ", nets: " + result.Nets.Count + " and pins: " + result.Pins.Count;
            Console.WriteLine(message);
            logger.LogMessage(message, LogCategory.INFO);
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
            Console.WriteLine(message);
            logger.LogMessage(message, LogCategory.INFO);
            sw.Stop();
            Debug.WriteLine("Parse all received GRPC objects into class objects: {0}ms", sw.Elapsed.TotalMilliseconds);
            return new ParsedResult(components, nets);
        }

        private byte[] GetZipFolderAsBytes(string pathToOdb)
        {
            if (!Directory.Exists(pathToOdb))
            {
                logger.LogMessage("Folder does not exist!" + pathToOdb, LogCategory.ERROR);
                return null;
            }

            try
            {
                ZipFile.CreateFromDirectory(pathToOdb, FILEPATH);
            }
            catch (IOException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return null;
            }
            catch (ArgumentException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return null;
            }
            catch (UnauthorizedAccessException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return null;
            }
            catch (NotSupportedException e)
            {
                logger.LogMessage(e.Message, LogCategory.ERROR);
                return null;
            }

            if (!File.Exists(FILEPATH))
            {
                logger.LogMessage("Zip file does not exist!" + FILEPATH, LogCategory.ERROR);
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
