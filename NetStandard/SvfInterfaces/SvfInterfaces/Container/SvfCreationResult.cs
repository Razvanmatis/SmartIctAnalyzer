using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.Svf.SvfInterfaces.Container
{
    public class SvfCreationResult
    {
        public SvfCreationResult(List<ISvfData> svfFiles, List<PinTestTypeContainer> allPinInformation)
        {
            SvfFiles = svfFiles;
            AllPinInformation = allPinInformation;
        }

        public List<ISvfData> SvfFiles { get; }

        public List<PinTestTypeContainer> AllPinInformation { get; }
    }
}
