using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Svf.SvfInterfaces.Container;
using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.Svf.Contracts;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProMik.SmartIct.Console.Contracts.Container
{
    public class SvfHandleResult : SvfCreationResult
    {
        public SvfHandleResult(
            List<ISvfData> svfData,
            List<PinTestTypeContainer> allPinInformation,
            BsdlContainer bsdlParameters) : base(svfData, allPinInformation) 
        {
            BsdlParameters = bsdlParameters;
        }

        public BsdlContainer BsdlParameters { get; }
    }
}
