namespace GrpcApi.Interfaces
{
    public interface IPcbInvestigatorApiHandler
    {
        IGrpcResult GetAllComponents(string steps);
    }
}
