namespace SVFHelper.Interfaces
{
    public interface ISvfData
    {
        string JtagName
        {
            get;
        }

        string PinName
        {
            get;
        }

        string BasePath
        {
            get;
        }

        string SvfRequestContent
        {
            get;
        }

        string SvfCheckContent
        {
            get;
        }

        string Prefix
        {
            get;
        }

        string Suffix
        {
            get;
        }

        bool Request
        {
            get;
        }

        bool Check
        {
            get;
        }

        string Mask
        {
            get;
        }

        string GetCompleteName();

        string GetCompleteFilePath();

        string GetCompleteRequestAsString();

        string GetCompleteCheckAsString();

        string GetCompleteContentAsString();
    }
}
