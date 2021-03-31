using Interfaces.Gui;
using SVFHelper.Interfaces;
using System.Collections.Generic;

namespace SVFHelper.Implementations
{
    public class PullUpSvfWriter : ISvfWriter
    {
        private readonly ISVFHelper svfHelper;

        public PullUpSvfWriter(ISVFHelper svfHelper) 
        {
            this.svfHelper = svfHelper;
        }

        public List<ISvfData> GetSVFFiles(string basePath, string jtagName, List<string> pinLabel, ProMik.BSDL.Interfaces.Entities.IBSDLOutput bsdlOutput, byte[] defaultVector)
        {
            List<ISvfData> data = new List<ISvfData>();
            foreach (var pin in pinLabel)
            {
                data.Add(svfHelper.GetSvfContent(basePath, jtagName, pin, bsdlOutput, false, true, defaultVector));
            }

            foreach (var pin in pinLabel)
            {
                data.Add(svfHelper.GetSvfContent(basePath, jtagName, pin, bsdlOutput, true, true, defaultVector));
            }

            return data;
        }
    }
}
