using GrpcApi;
using PcbInvestigatorApiV11.Implementations;

namespace PcbInvestigatorApiV11
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            MainGrpcProgramm main = new MainGrpcProgramm(new PcbInvestigatorApiConverter(), new PcbInvestigatorApiHandler());
            main.StartServer(args, "PCB Investigator V11.2 API");
        }
    }
}
