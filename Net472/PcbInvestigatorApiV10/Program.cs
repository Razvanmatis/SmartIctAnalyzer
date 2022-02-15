using GrpcApi;
using PcbInvestigatorApiV10.Implementations;
using System;

namespace PcbInvestigatorApiV10
{
    class Program
    {
        static void Main(string[] args)
        {
            MainGrpcProgramm main = new MainGrpcProgramm(new PcbInvestigatorApiConverter(), new PcbInvestigatorApiHandler());
            main.StartServer(args, "PCB Investigator V10.2 API");
        }
    }
}
