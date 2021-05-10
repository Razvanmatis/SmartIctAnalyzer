namespace SVFHelper.Interfaces
{
    public interface ISvfHandler
    {
        void HandleSvfFileGeneration(ISvfExporter projectHandlerToUse, string jtagPinInformation = "");

        void PlaySvfFileHandler();
    }
}
