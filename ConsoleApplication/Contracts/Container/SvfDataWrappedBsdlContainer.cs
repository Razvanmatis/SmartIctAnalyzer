using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.Svf.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.Console.Contracts.Container
{
    public class SvfDataWrappedBsdlContainer : WrappedBsdlContainer
    {
        public SvfDataWrappedBsdlContainer(
            BsdlContainer bsdlContainerToUse,
            string bsdlFileToBeUsed,
            List<ISvfData> svfDataFiles) : base(bsdlContainerToUse, bsdlFileToBeUsed)
        {
            SvfDataFiles = svfDataFiles;
        }

        public List<ISvfData> SvfDataFiles { get; }
    }
}
