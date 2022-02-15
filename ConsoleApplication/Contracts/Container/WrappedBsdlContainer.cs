using ProMik.Svf.Contracts;
using System.Collections.Generic;

namespace ProMik.SmartIct.Console.Contracts.Container
{
    public class WrappedBsdlContainer
    {
        public WrappedBsdlContainer(
            BsdlContainer bsdlContainerToUse,
            string bsdlFileToBeUsed)
        {
            BsdlContainerToUse = bsdlContainerToUse;
            BSDLFileToBeUsed = bsdlFileToBeUsed;
        }

        public BsdlContainer BsdlContainerToUse { get; }

        public string BSDLFileToBeUsed { get; }

       
    }
}
