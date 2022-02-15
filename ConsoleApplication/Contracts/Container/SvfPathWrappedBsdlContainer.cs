using ProMik.Svf.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.Console.Contracts.Container
{
    public class SvfPathWrappedBsdlContainer : WrappedBsdlContainer
    {
        public SvfPathWrappedBsdlContainer(
            BsdlContainer bsdlContainerToUse,
            string bsdlFileToBeUsed,
            SvfPath svfPathsToUseFromLocalSystem) : base(bsdlContainerToUse, bsdlFileToBeUsed)
        {
            SvfPathsToUseFromLocalSystem = svfPathsToUseFromLocalSystem;
        }
        public SvfPath SvfPathsToUseFromLocalSystem { get; }
    }
}
