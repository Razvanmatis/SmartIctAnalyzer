namespace GrpcApi.Interfaces
{
    public interface IPcbInvestigatorApiHandler
    {
        string PathToOdb { get; set; }

        IPcbInvestigatorApiBaseConverter Converter { get; set; }

        IGrpcResult GetAllComponents(string steps);
    }
}
