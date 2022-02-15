using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Grpc.Core;
using Ionic.Zip;
using ProMik.SmartIct.Interfaces.GrpcClientParser;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.PCBComponentParser.Helper.JsonObjects;
using ProMik.SmartIct.PCBComponentParser.Interfaces;
using static PcbInvestigatorService;

namespace ProMik.SmartIct.PCBComponentParser.Implementations
{
    public class PCBComponentParserHandler : IPCBComponentParser
    {
        private const string PASSWORD = "ProMik@100!";
        private const int PORT = 50151;
        private const string DEFIP = "127.0.0.1";
        private static readonly string FILEPATH = Directory.GetCurrentDirectory() + "\\grpcRequest.zip";
        private static readonly Func<IFunctionalAttributes, bool> ISTESTPOINTDEF = new Func<IFunctionalAttributes, bool>(x =>
            x.PartName.Contains("testpoint") || x.PackageName.Contains("TP") || x.PartName.Contains("TP"));

        private static PcbInvestigatorServiceClient client;
        private readonly ILogger logger;
        private Channel channel;

        public PCBComponentParserHandler(ILogger logger)
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

                StatusResult result = await client.ClientAvailableAsync(
                    new Empty(), new CallOptions(deadline: DateTime.UtcNow.AddSeconds(10)));
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
            IList<string> conIdentifier = null,
            bool useContains = false)
        {
            Request request = new Request
            {
                PathToOdb = pathToOdb,
                Steps = steps,
            };
            Result result = await client.GetConvertedObjectsAsync(request);
            return FinalizeHandling(
                result, rIdentifier, cIdentifier, iIdentifier, tpIdentifier, icIdentifier, conIdentifier, useContains);
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
            IList<string> conIdentifier = null,
            bool useContains = false)
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
                logger,
                useContains);
            sw.Stop();
            Debug.WriteLine("Stage IMPORT: {0}ms", sw.Elapsed.TotalMilliseconds);
            return result;
        }

        public async Task<IParsedResult> GetParsedObjectsFromGrpcByZipFolder(
            byte[] odbContentAsZip,
            string steps,
            IList<string> rIdentifier = null,
            IList<string> cIdentifier = null,
            IList<string> iIdentifier = null,
            IList<string> tpIdentifier = null,
            IList<string> icIdentifier = null,
            IList<string> conIdentifier = null,
            bool useContains = false)
        {
            RequestZip request = new RequestZip()
            {
                ZipFolder = ByteString.CopyFrom(odbContentAsZip),
                Steps = steps,
                ComputerName = Environment.MachineName,
            };
            Result result = await client.GetConvertedObjectsByZipAsync(request);
            return FinalizeHandling(
                result, rIdentifier, cIdentifier, iIdentifier, tpIdentifier, icIdentifier, conIdentifier, useContains);
        }

        public async Task<IParsedResult> GetParsedObjectsFromGrpcByZipFolder(
            string pathToOdb,
            string steps,
            IList<string> rIdentifier = null,
            IList<string> cIdentifier = null,
            IList<string> iIdentifier = null,
            IList<string> tpIdentifier = null,
            IList<string> icIdentifier = null,
            IList<string> conIdentifier = null,
            bool useContains = false)
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
                ComputerName = Environment.MachineName,
            };
            Result result = await client.GetConvertedObjectsByZipAsync(request);
            return FinalizeHandling(
                result, rIdentifier, cIdentifier, iIdentifier, tpIdentifier, icIdentifier, conIdentifier, useContains);
        }

        private static int GetTotalPinAmount(IList<IPCBComponent> components)
        {
            HashSet<IPinComponent> pins = new HashSet<IPinComponent>();
            foreach (var comp in components)
            {
                foreach (var pin in comp.Connections)
                {
                    pins.Add(pin);
                }
            }

            return pins.Count;
        }

        private static Func<IFunctionalAttributes, bool> GetTpFunction(IList<string> tpIdentifier)
        {
            return new Func<IFunctionalAttributes, bool>(x =>
            {
                return tpIdentifier.FirstOrDefault(text =>
                    x.PartName.ToLower(CultureInfo.CurrentCulture).Contains(text.ToLower(CultureInfo.CurrentCulture))
                    || x.PackageName.ToLower(CultureInfo.CurrentCulture).Contains(text.ToLower(CultureInfo.CurrentCulture))) != null;
            });
        }

        private IParsedResult FinalizeHandling(
            Result result,
            IList<string> rIdentifier,
            IList<string> cIdentifier,
            IList<string> iIdentifier,
            IList<string> tpIdentifier,
            IList<string> icIdentifier,
            IList<string> conIdentifier,
            bool useContains = false)
        {
            Stopwatch sw = Stopwatch.StartNew();
            string message =
                "Received from GRPC amount PCB Components: "
                + result.Components.Count + ", nets: " + result.Nets.Count + " and pins: " + result.Pins.Count;
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
                isTestPoint,
                useContains);
            IList<INetComponent> nets = JsonImportExportHelper.GetParsedNets(components);

            int pinAmount = GetTotalPinAmount(components);
            message = "After parsing creation of total objects PCB Components: "
                + components.Count + ", nets: " + nets.Count + " and pins: " + pinAmount;
            Console.WriteLine(message);
            logger.LogMessage(message, LogCategory.INFO);
            sw.Stop();
            Debug.WriteLine("Parse all received GRPC objects into class objects: {0}ms", sw.Elapsed.TotalMilliseconds);
            return new ParsedResult(components, nets, resultJson.StepAmount);
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
                using (ZipFile zip = new ZipFile())
                {
                    zip.Password = PASSWORD;
                    zip.AddDirectory(pathToOdb);
                    zip.Save(FILEPATH);
                }
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
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            channel = new Channel(
                ip,
                PORT,
                ChannelCredentials.Insecure,
                new List<ChannelOption>()
                {
                    new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue),
                    new ChannelOption(ChannelOptions.MaxSendMessageLength, int.MaxValue),
                });
            client = new PcbInvestigatorServiceClient(channel);
        }
    }
}
