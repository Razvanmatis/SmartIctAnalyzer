namespace SVFHelper.Interfaces
{
    public interface ISvfHandler
    {
        void HandleSvfFileGeneration(ISvfExporter projectHandlerToUse);

        void PlaySvfFileHandler();
    }
}
