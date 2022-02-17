using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Grpc.Core;
using GrpcApi.Interfaces;
using static PcbInvestigatorService;

namespace GrpcApi
{
    public class MainGrpcProgramm
    {
      // private const bool TESTMODE = false;
        private const int PORT = 50151;
        private readonly IPcbInvestigatorApiBaseConverter converter;
        private readonly IPcbInvestigatorApiHandler apiHandler;

        public MainGrpcProgramm(IPcbInvestigatorApiBaseConverter converter, IPcbInvestigatorApiHandler apiHandler)
        {
            this.converter = converter;
            this.apiHandler = apiHandler;
        }

        public static void Main(string[] args)
        {
        }

        public void StartServer(string[] args, string text)
        {
            if (args != null && args.Length > 0)
            {
                Console.WriteLine("Received arguments: " + args.ToString());
            }

            PcbInvestigatorServiceBase impl = new PcbInvestigatorServiceImpl(converter, apiHandler);
            Server server = new Server(
                new List<ChannelOption>()
                {
                    new ChannelOption(ChannelOptions.MaxReceiveMessageLength, int.MaxValue),
                    new ChannelOption(ChannelOptions.MaxSendMessageLength, int.MaxValue),
                })
            {
                Services = { PcbInvestigatorService.BindService(impl) },
                Ports = { new ServerPort("0.0.0.0", PORT, ServerCredentials.Insecure), },
            };
            server.Start();
            try
            {
                Console.WriteLine("SmartIct Tool GRPC server listening on port " + PORT);
                Console.WriteLine(text + ": " + LangRessource.Version);
            }
            catch (IOException e)
            {
                Console.WriteLine("Error because of " + e.Message);
            }

          /*  if (TESTMODE)
            {
                string pathToOdb = "C:\\Repositories\\smart_ict_analyser\\Testdaten\\panel";
                Request request = new Request();
                request.PathToOdb = pathToOdb;
                var objects = impl.GetConvertedObjects(request, new DummyServerCallContext()).Result;
            } */

            while (true)
            {
                Thread.Sleep(2000);
            }
        }
    }
}
