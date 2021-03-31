using System;
using System.IO;
using System.Threading;
using Grpc.Core;
using static PcbInvestigatorService;

namespace GrpcApi
{
    public static class Programm
    {
      // private const bool TESTMODE = false;
        private const int PORT = 50151;

        public static void Main(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                Console.WriteLine("Received arguments: " + args.ToString());
            }

            PcbInvestigatorServiceBase impl = new PcbInvestigatorServiceImpl();
            Server server = new Server()
            {
                Services = { PcbInvestigatorService.BindService(impl) },
                Ports = { new ServerPort("0.0.0.0", PORT, ServerCredentials.Insecure), },
            };
            server.Start();
            try
            {
                Console.WriteLine("SmartIct Tool GRPC server listening on port " + PORT);
                Console.WriteLine(LangRessource.Version);
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
