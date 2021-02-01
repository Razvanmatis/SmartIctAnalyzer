using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grpc.Core;
using GrpcApi.Handler;
using GrpcApi.Interfaces;
using Interfaces.PcbInvestigator;

namespace GrpcApi
{
    public class PcbInvestigatorServiceImpl : PcbInvestigatorService.PcbInvestigatorServiceBase
    {
        private static readonly string FILEPATHREQUEST = Directory.GetCurrentDirectory() + "\\grpcRequest.zip";
        private static readonly string FILEPATHEXTRACT = Directory.GetCurrentDirectory() + "\\grpcRequestExtracted";

        public override async Task<StatusResult> ClientAvailable(Empty request, ServerCallContext context)
        {
            StatusResult result = new StatusResult();
            result.Status = true;
            return await Task.FromResult(result).ConfigureAwait(true);
        }

        public override async Task<Result> GetConvertedObjectsByZip(RequestZip request, ServerCallContext context)
        {
            if (request.ZipFolder == null || request.ZipFolder.Length == 0)
            {
                Console.WriteLine("Error by receiving empty zip folder data!");
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }

            byte[] data = request.ZipFolder.ToByteArray();
            try
            {
                FileStream stream = File.Create(FILEPATHREQUEST);
                stream.Write(data, 0, data.Length);
                stream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }

            try
            {
                ZipFile.ExtractToDirectory(FILEPATHREQUEST, FILEPATHEXTRACT);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return await Task.FromResult(new Result()).ConfigureAwait(true);
            }

            Result result = await FinishParsing(FILEPATHEXTRACT).ConfigureAwait(true);
            if (result != null)
            {
                try
                {
                    Directory.Delete(FILEPATHEXTRACT, true);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                try
                {
                    File.Delete(FILEPATHREQUEST);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }

                return await Task.FromResult(result).ConfigureAwait(true);
            }
            else
            {
                Console.WriteLine("Error getting parsed objects by folder!");
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

            return await FinishParsing(request.PathToOdb).ConfigureAwait(true);
        }

        private static async Task<Result> FinishParsing(string pathToOdb)
        {
            IPcbInvestigatorApiConverter converter = new PcbInvestigatorApiConverter();
            IPcbInvestigatorApiHandler apiHandler = new PcbInvestigatorApiHandler(pathToOdb, converter);
            IGrpcResult components = apiHandler.GetAllComponents();
            IList<PinGrpc> pinsGrpc = converter.GetPinsGrpc(components.AllPins, components.AllNets);
            IList<ComponentGrpc> componentsGrpc = converter.GetComponentsGrpc(components.AllComponents, components.AllPins);
            IList<NetGrpc> netsGrpc = converter.GetNetsGrpc(components.AllPins, components.AllComponents, components.AllNets);
            List<string> layerNames = new List<string>();
            foreach (var comp in componentsGrpc)
            {
                if (!layerNames.Contains(comp.Functionals.LayerName))
                {
                    layerNames.Add(comp.Functionals.LayerName);
                }
            }

            Console.WriteLine("Sending back as GRPC result PCB components" + componentsGrpc.Count + ", nets: " + netsGrpc.Count + " and pins: " + pinsGrpc.Count
                 + " within layers: " + layerNames.Count);
            return await Task.FromResult(GetResult(pinsGrpc, componentsGrpc, netsGrpc)).ConfigureAwait(true);
        }

        private static Result GetResult(IList<PinGrpc> pinsGrpc, IList<ComponentGrpc> componentsGrpc, IList<NetGrpc> netsGrpc)
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

            return result;
        }
    }
}
