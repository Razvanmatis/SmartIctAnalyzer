using ProMik.BSDL.Interfaces;
using ProMik.BSDL.Interfaces.Entities;
using SVFHelper.Interfaces;

namespace SVFHelper.Implementations
{
    public class PackageService : IPackageService
    {
        private readonly IBSDLProcessor bsdlProcessor;

        public PackageService(IBSDLProcessor bsdlProcessor)
        {
            this.bsdlProcessor = bsdlProcessor;
        }

        public IBSDLOutput GetPackage(string path)
        {
            return bsdlProcessor.ParseBSDLFile(path).Result;
        }
    }
}