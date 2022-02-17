using ProMik.Svf.Contracts;
using System;
using System.Collections.Generic;

namespace ProMik.SmartIct.Svf.SvfInterfaces.Interfaces
{
    public enum MultiPinType
    {
        None,
        Neighbours,
        SameNetConnections,
        SameType
    }

    public interface ISvfData : ICloneable
    {
        string JtagName
        {
            get;
        }

        string PinName
        {
            get;
        }

        string PinPort { get; }

        string BasePath
        {
            get;
            set;
        }

        bool TdiControlInput
        {
            get;
        }

        bool ExpectedValue
        {
            get;
        }

        string TdiString { get; set; }

        string TdoString { get; set; }

        string Mask { get; set; }

        bool PinNameNotFoundAtAll
        {
            get;
        }

        Dictionary<int, string> SafeValues { get; }

        List<string> Content
        {
            get;
        }

        bool SafeValuesAreMatchingExpectedValue();

        List<int> MaskPositions
        {
            get;
        }

        List<ISvfData> Neighbours { get; }

        List<string> ConnectedComponents { get; }

        BoundaryScanTestType TestType { get; set; }

        BoundaryScanTestType BasedTestTypeForCloning { get; set; }

        string GetCompleteName();

        string GetCompleteFilePath();

        string GetCompleteContentAsString();

        List<SvfPinInfo> GetPinInformation();

        MultiPinType MultiPinType { get; set; }

        ISvfWriter SvfWriter { get; set; }
    }
}
