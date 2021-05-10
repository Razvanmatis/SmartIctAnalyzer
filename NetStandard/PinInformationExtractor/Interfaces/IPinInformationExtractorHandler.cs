namespace PinInformationExtractor.Interfaces
{
    public interface IPinInformationExtractorHandler
    {
        void GetPinInformationJtagsIntoFile(string jtagPinInformation = PinInformationExtractor.Implementations.PinInformationExtractor.DEFJTAGPINIDENTIFIER);
    }
}
