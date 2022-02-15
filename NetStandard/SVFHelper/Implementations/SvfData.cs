using ProMik.SmartIct.Svf.SvfInterfaces.Interfaces;
using ProMik.Svf.Contracts;
using System;
using System.Collections.Generic;
using System.IO;

namespace ProMik.SmartIct.Svf.SvfFileCreation.Implementations
{
    public class SvfData : ISvfData
    {
        private const string SVFENDING = ".svf";

        public SvfData(string jtagName,
            string pinName,
            string basePath,
            bool pinNameNotFoundAtAll,
            ISvfWriter writer) : this(
                jtagName,
                pinName,
                basePath,
                new List<string>(),
                false,
                false,
                pinNameNotFoundAtAll,
                new List<int>(),
                new List<string>(),
                string.Empty,
                BoundaryScanTestType.Undefined,
                string.Empty,
                string.Empty,
                string.Empty,
                new Dictionary<int, string>(),
                writer)
        {
        }

        public SvfData(
            string jtagName,
            string pinName,
            string basePath,
            List<string> content,
            bool tdiControlInput,
            bool expectedValue,
            bool pinNameNotFoundAtAll,
            List<int> maskPositions,
            List<string> connectedComponents,
            string pinPort,
            BoundaryScanTestType testType,
            string tdiString,
            string tdoString,
            string mask,
            Dictionary<int, string> safeValues,
            ISvfWriter writer,
            BoundaryScanTestType basedTestTypeForCloning = BoundaryScanTestType.Undefined)
        {
            JtagName = jtagName;
            PinName = pinName;
            BasePath = basePath;
            Content = content;
            TdiControlInput = tdiControlInput;
            ExpectedValue = expectedValue;
            PinNameNotFoundAtAll = pinNameNotFoundAtAll;
            MaskPositions = maskPositions;
            ConnectedComponents = connectedComponents;
            PinPort = pinPort;
            TestType = testType;
            TdiString = tdiString;
            TdoString = tdoString;
            Mask = mask;
            BasedTestTypeForCloning = basedTestTypeForCloning;
            SafeValues = safeValues;
            MultiPinType = MultiPinType.None;
            SvfWriter = writer;
        }

        public List<string> ConnectedComponents { get; private set; }

        public bool PinNameNotFoundAtAll { get; private set; }

        public List<int> MaskPositions { get; private set; }

        public string JtagName { get; private set; }

        public string PinName { get; private set; }

        public string BasePath { get; set; }

        public List<string> Content { get; private set; }

        public bool TdiControlInput { get; private set; }

        public bool ExpectedValue { get; private set; }

        public string PinPort { get; }

        public List<ISvfData> Neighbours { get; } = new List<ISvfData>();

        public BoundaryScanTestType TestType { get; set; } = BoundaryScanTestType.Undefined;

        public string TdiString { get; set; }

        public string TdoString { get; set; }

        public string Mask { get; set; }

        public BoundaryScanTestType BasedTestTypeForCloning { get; set; } = BoundaryScanTestType.Undefined;

        public Dictionary<int, string> SafeValues { get; } = new Dictionary<int, string>();

        public MultiPinType MultiPinType { get; set; }

        public ISvfWriter SvfWriter { get; set; }

        public string GetCompleteName()
        {
            string value = string.Empty;
            if (MultiPinType == MultiPinType.None
                || MultiPinType == MultiPinType.Neighbours
                || MultiPinType == MultiPinType.SameNetConnections)
            {
                value = (JtagName + "_" + PinName + "_" + (TdiControlInput ? "IN" : "OUT") + "_"
                    + (ExpectedValue ? "HI" : "LO")).Replace(":", "_").Replace("-", "_");
                if (MultiPinType == MultiPinType.Neighbours)
                {
                    value += Neighbours.Count == 0 ? "" : "_neighbours_" + Neighbours.Count;
                }
                else if (MultiPinType == MultiPinType.SameNetConnections)
                {
                    value += Neighbours.Count == 0 ? "" : "_sameNets_" + Neighbours.Count;
                }
            }
            else
            {
                value = (JtagName + "_" + (TdiControlInput ? "IN" : "OUT") + "_"
                    + (ExpectedValue ? "HI" : "LO")).Replace(":", "_").Replace("-", "_");
                value += Neighbours.Count == 0 ? "" : "_sameType_" + Enum.GetName(typeof(BoundaryScanTestType),
                    BasedTestTypeForCloning) + "_" + (Neighbours.Count + 1);
            }

            return value + SVFENDING;
        }

        public string GetCompleteFilePath()
        {
            return Path.Combine(BasePath, JtagName.Replace(":", "_"), SvfWriter.DestinationPath, GetCompleteName());
        }

        public string GetCompleteContentAsString()
        {
            return string.Join("\n", Content);
        }

        public List<SvfPinInfo> GetPinInformation()
        {
            List<SvfPinInfo> list = new List<SvfPinInfo>();
            if (Neighbours.Count == 0)
            {
                list.Add(GetPinInfo());
            }
            else
            {
                list.Add(GetPinInfo());
                Neighbours.ForEach(svf => list.AddRange(svf.GetPinInformation()));
            }

            return list;
        }

        public object Clone()
        {
            return new SvfData(
                JtagName,
                PinName,
                BasePath,
                new List<string>(Content),
                TdiControlInput,
                ExpectedValue,
                PinNameNotFoundAtAll,
                new List<int>(MaskPositions),
                new List<string>(ConnectedComponents),
                PinPort,
                TestType,
                TdiString,
                TdoString,
                Mask,
                new Dictionary<int, string>(SafeValues),
                SvfWriter,
                BasedTestTypeForCloning);
        }

        private SvfPinInfo GetPinInfo()
        {
            return new SvfPinInfo()
            {
                MaskPosition = MaskPositions.Count > 0 ? (uint)MaskPositions[0] : 0,
                PinName = PinName,
                TestType = TestType,
                ConnectedComponents = ConnectedComponents,
                PinPort = PinPort,
            };
        }

        public bool SafeValuesAreMatchingExpectedValue()
        {
            foreach (var pos in MaskPositions)
            {
                if (!SafeValues.ContainsKey(pos))
                {
                    return false;
                }

                if (!(ExpectedValue && SafeValues[pos].Equals("1") || !ExpectedValue && SafeValues[pos].Equals("0")))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
