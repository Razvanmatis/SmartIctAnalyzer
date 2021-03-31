using ProMik.BSDL.Interfaces.Entities;

namespace SVFHelper.Interfaces
{
    public interface IPackageService
    {
        IBSDLOutput GetPackage(string path);
    }
}
