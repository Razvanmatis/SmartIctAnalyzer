using ProMik.BSDL.Interfaces.Entities;
using SVFHelper.Interfaces;
using System.Collections.Generic;

namespace SVFHelper.Implementations
{
    public class UnknownComponentsSvfWriter : ISvfWriter
    {
        private readonly ISVFHelper svfHelper;

        public UnknownComponentsSvfWriter(ISVFHelper svfHelper)
        {
            this.svfHelper = svfHelper;
        }

        public List<ISvfData> GetSVFFiles(string basePath, string jtagName, List<string> pinLabel, IBSDLOutput bsdlOutput, byte[] defaultVector)
        {
            List<ISvfData> data = new List<ISvfData>();
            foreach (var pin in pinLabel)
            {
                data.Add(svfHelper.GetSvfContent(basePath, jtagName, pin, bsdlOutput, false, false, defaultVector));
            }

            foreach (var pin in pinLabel)
            {
                data.Add(svfHelper.GetSvfContent(basePath, jtagName, pin, bsdlOutput, true, true, defaultVector));
            }

            return data;
        }
    }
}
