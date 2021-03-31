using SVFHelper.Interfaces;
using System;

namespace SVFHelper.Implementations
{
    public class SvfData : ISvfData
    {
        public SvfData(string jtagName, string pinName, string basePath, string svfRequestContent, string svfCheckContent, string prefix, string suffix, bool request, bool check, string mask)
        {
            JtagName = jtagName;
            PinName = pinName;
            BasePath = basePath;
            SvfRequestContent = svfRequestContent;
            SvfCheckContent = svfCheckContent;
            Prefix = prefix;
            Suffix = suffix;
            Request = request;
            Check = check;
            Mask = mask;
        }

        public string JtagName { get; private set; }

        public string PinName { get; private set; }

        public string BasePath { get; private set; }

        public string SvfRequestContent { get; private set; }

        public string SvfCheckContent { get; private set; }

        public string Prefix { get; private set; }

        public string Suffix { get; private set; }

        public bool Request { get; private set; }

        public bool Check { get; private set; }

        public string Mask { get; private set; }

        public string GetCompleteName()
        {
            return ("svfExport_" + JtagName + "_" + PinName + "_" + (Request ? "HI" : "LO") + "_" + (Check ? "HI" : "LO")).Replace(":", "_").Replace("-", "_");
        }

        public string GetCompleteFilePath()
        {
            string basepath = BasePath;
            if (!basepath.EndsWith("\\", StringComparison.InvariantCulture))
            {
                basepath += "\\";
            }

            basepath += GetCompleteName();
            return basepath;
        }

        public string GetCompleteRequestAsString()
        {
            return Prefix + Suffix + SvfRequestContent + ");";
        }

        public string GetCompleteCheckAsString()
        {
            return Prefix + Suffix + SvfCheckContent + ");";
        }

        public string GetCompleteContentAsString()
        {
            return Prefix + Suffix + SvfRequestContent + ")\nTDO (" + SvfCheckContent + ")\nMASK (" + Mask + ");";
        }
    }
}
