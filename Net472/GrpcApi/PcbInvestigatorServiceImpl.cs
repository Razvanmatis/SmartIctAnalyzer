using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcApi.Interfaces;
using Ionic.Zip;

namespace GrpcApi
{
    public class PcbInvestigatorServiceImpl : PcbInvestigatorService.PcbInvestigatorServiceBase
    {
        private const string PASSWORD = "ProMik@100!";
        private static readonly string FILEPATHREQUEST = Directory.GetCurrentDirectory() + "\\grpcRequest.zip";
        private static readonly string FILEPATHEXTRACT = Directory.GetCurrentDirectory() + "\\grpcRequestExtracted";
        private readonly IPcbInvestigatorApiBaseConverter converter;
        private readonly IPcbInvestigatorApiHandler apiHandler;

        public PcbInvestigatorServiceImpl(IPcbInvestigatorApiBaseConverter converter, IPcbInvestigatorApiHandler apiHandler)
        {
            this.converter = converter;
            this.apiHandler = apiHandler;
            this.apiHandler.Converter = converter;
        }

        public override async Task<StatusResult> ClientAvailable(Empty request, ServerCallContext context)
        {
            StatusResult result = new StatusResult
            {
                Status = true,
            };
            return await Task.FromResult(result).ConfigureAwait(true);
        }

        public override async Task<Result> GetConvertedObjectsByZip(RequestZip request, ServerCallContext context)
        {
            Stopwatch sw = Stopwatch.StartNew();
            if (request.ZipFolder == null || request.ZipFolder.Length == 0)
            {
                Console.WriteLine(LangRessource.ErrorByReceivingEmptyZipFolder);
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }

            if (File.Exists(FILEPATHREQUEST + request.ComputerName))
            {
                try
                {
                    File.Delete(FILEPATHREQUEST + request.ComputerName);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                    return await Task.FromResult(new Result()).ConfigureAwait(true);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                    return await Task.FromResult(new Result()).ConfigureAwait(true);
                }
                catch (NotSupportedException e)
                {
                    Console.WriteLine(e.Message);
                    return await Task.FromResult(new Result()).ConfigureAwait(true);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                    return await Task.FromResult(new Result()).ConfigureAwait(true);
                }
            }

            byte[] data = request.ZipFolder.ToByteArray();
            try
            {
                FileStream stream = File.Create(FILEPATHREQUEST + request.ComputerName);
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }
            catch (NotSupportedException e)
            {
                Console.WriteLine(e.Message);
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }

            if (Directory.Exists(FILEPATHEXTRACT + request.ComputerName))
            {
                try
                {
                    Directory.Delete(FILEPATHEXTRACT + request.ComputerName, true);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }
            }

            try
            {
                using (ZipFile file = ZipFile.Read(FILEPATHREQUEST + request.ComputerName))
                {
                    if (ZipFile.CheckZipPassword(FILEPATHREQUEST + request.ComputerName, PASSWORD))
                    {
                        file.Password = PASSWORD;
                    }
                    else
                    {
                        Console.WriteLine(LangRessource.UsedOtherPassword);
                        return new Result();
                    }

                    file.ExtractAll(FILEPATHEXTRACT + request.ComputerName);
                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine(e.Message);
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }
            catch (UnauthorizedAccessException e)
            {
                Console.WriteLine(e.Message);
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }
            catch (NotSupportedException e)
            {
                Console.WriteLine(e.Message);
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }
            catch (InvalidDataException e)
            {
                Console.WriteLine(e.Message);
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }

            sw.Stop();
            Console.WriteLine(LangRessource.DecompressZippedContent, sw.Elapsed.TotalMilliseconds);
            Result result = await FinishParsing(FILEPATHEXTRACT + request.ComputerName, request.Steps).ConfigureAwait(true);
            if (result != null)
            {
                try
                {
                    Directory.Delete(FILEPATHEXTRACT + request.ComputerName, true);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                }

                try
                {
                    File.Delete(FILEPATHREQUEST + request.ComputerName);
                }
                catch (IOException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (NotSupportedException e)
                {
                    Console.WriteLine(e.Message);
                }
                catch (UnauthorizedAccessException e)
                {
                    Console.WriteLine(e.Message);
                }

                return await Task.FromResult(result).ConfigureAwait(true);
            }
            else
            {
                Console.WriteLine(LangRessource.ErrorGettingParsedObjectsByFolder);
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }
        }

        public override async Task<Result> GetConvertedObjects(Request request, ServerCallContext context)
        {
            Console.WriteLine("Received request for odb path: " + request.PathToOdb);
            if (string.IsNullOrEmpty(request.PathToOdb))
            {
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }

            return await FinishParsing(request.PathToOdb, request.Steps).ConfigureAwait(true);
        }

        private static Result GetResult(
            IList<PinGrpc> pinsGrpc, IList<ComponentGrpc> componentsGrpc, IList<NetGrpc> netsGrpc, int stepAmount)
        {
            Result result = new Result();
            for (int idx = 0; idx < pinsGrpc.Count; idx++)
            {
                result.Pins.Add(idx, pinsGrpc[idx]);
            }

            for (int idx = 0; idx < componentsGrpc.Count; idx++)
            {
                result.Components.Add(idx, componentsGrpc[idx]);
            }

            for (int idx = 0; idx < netsGrpc.Count; idx++)
            {
                result.Nets.Add(idx, netsGrpc[idx]);
            }

            result.AmountSteps = stepAmount;

            return result;
        }

        private async Task<Result> FinishParsing(string pathToOdb, string steps)
        {
            apiHandler.PathToOdb = pathToOdb;
            IGrpcResult components = apiHandler.GetAllComponents(steps);
            IList<PinGrpc> pinsGrpc = converter.GetPinsGrpc(components.AllPins, components.AllNets);
            IList<ComponentGrpc> componentsGrpc = converter.GetComponentsGrpc(components.AllComponents, components.AllPins);
            IList<NetGrpc> netsGrpc = converter.GetNetsGrpc(components.AllPins, components.AllComponents, components.AllNets);
            HashSet<string> layerNames = new HashSet<string>();
            foreach (var comp in componentsGrpc)
            {
                layerNames.Add(comp.Functionals.LayerName);
            }

            Console.WriteLine(
                "Sending back as GRPC result PCB components"
                + componentsGrpc.Count
                + ", nets: "
                + netsGrpc.Count
                + " and pins: "
                + pinsGrpc.Count
                + " within layers: "
                + layerNames.Count);
            return await Task.FromResult(GetResult(pinsGrpc, componentsGrpc, netsGrpc, components.StepAmount))
                .ConfigureAwait(true);
        }
    }
}
