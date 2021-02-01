namespace TestDotNetCore
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using GrpcClientParser;
    using GrpcClientParser.Helper;
    using GrpcClientParser.Interfaces;
    using Interfaces.PcbInvestigator;
    using Interfaces.UnitTests;
    using Newtonsoft.Json;
    using Ui.Modules.ModuleName.Helper;
    using Xunit;

    /// <summary>
    /// Test for parser logic.
    /// </summary>
    public class TestParserLogic
    {
        private const string PATHTOODB = "C:\\Repositories\\smart_ict_analyser\\Testdaten\\panel";
        private static string filePath = Directory.GetCurrentDirectory() + "\\exportApiObjects.txt";

        /// <summary>
        /// TestParsingFromApi.
        /// </summary>
        [Fact]
        public void TestParsingFromApi()
        {
            IGrpcClientParserHandler grpcHandler = new GrpcClientParserHandler();
            List<PcbTestObject> resultData = GetSerialzedData(filePath);
            Assert.NotNull(resultData);
            Assert.True(resultData.Count > 0);
            var result = grpcHandler.GetParsedObjectsFromGrpc(PATHTOODB).Result;
            Assert.NotNull(result);
            Assert.True(result.Components.Count > 0);
            Assert.True(result.Nets.Count > 0);
            TestContent(resultData, result);
            Debug.WriteLine("Tests finished sucessfully!");
        }

        private static List<PcbTestObject> GetSerialzedData(string fileToUse)
        {
            string content = string.Empty;
            try
            {
                content = File.ReadAllText(fileToUse);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error reading out the export file: " + fileToUse + ": " + e.Message);
                return null;
            }

            if (string.IsNullOrEmpty(content))
            {
                Debug.WriteLine("Export file is empty!");
                return null;
            }

            List<PcbTestObject> data = null;
            try
            {
                data = JsonConvert.DeserializeObject<List<PcbTestObject>>(content);
            }
            catch (Exception e)
            {
                Debug.WriteLine("Error parsing export file: " + e.Message);
                return null;
            }

            return data;
        }

        private static void TestContent(List<PcbTestObject> resultData, IParsedResult result)
        {
            Assert.True(resultData.Count == result.Components.Count);
            int netAmountFromTesting = GetAllNetsCountFromTestData(resultData);
            Assert.True(netAmountFromTesting == result.Nets.Count);
            foreach (var data in resultData)
            {
                IPCBComponent comp = GetComponentFromList(data.Name, result.Components);
                Assert.NotNull(comp);
                Assert.True(data.Pins.Count == comp.Connections.Count);
                Assert.True(AreAllNetsIncluded(data.Pins, comp));
            }
        }

        private static bool AreAllNetsIncluded(List<PinTestObject> pins, IPCBComponent comp)
        {
            List<string> netsOfParsed = GetNetsOfParsedObject(comp.Connections);
            List<string> netsOfTestObject = GetNetsOfTestObject(pins);
            if (netsOfTestObject.Count != netsOfParsed.Count)
            {
                Debug.WriteLine("Missmatch net amount for component with name " + comp.FunctionalAttributes.Ref + " " + netsOfTestObject.Count + ":" + netsOfParsed.Count);
                return false;
            }

            foreach (var net in netsOfTestObject)
            {
                if (!netsOfParsed.Contains(net))
                {
                    Debug.WriteLine("Net not included with name: " + net + " in component with name " + comp.FunctionalAttributes.Ref);
                    return false;
                }
            }

            return true;
        }

        private static List<string> GetNetsOfTestObject(List<PinTestObject> pins)
        {
            List<string> nets = new List<string>();
            foreach (var pin in pins)
            {
                foreach (var net in pin.Nets)
                {
                    if (!nets.Contains(net))
                    {
                        nets.Add(net);
                    }
                }
            }

            return nets;
        }

        private static List<string> GetNetsOfParsedObject(IList<IPinComponent> connections)
        {
            List<string> nets = new List<string>();
            foreach (var pin in connections)
            {
                foreach (var net in pin.Nets)
                {
                    if (!nets.Contains(net.NetName))
                    {
                        nets.Add(net.NetName);
                    }
                }
            }

            return nets;
        }

        private static IPCBComponent GetComponentFromList(string name, IList<IPCBComponent> components)
        {
            foreach (var comp in components)
            {
                if (name.Equals(comp.FunctionalAttributes.Ref))
                {
                    return comp;
                }
            }

            Debug.WriteLine("Component not found with name: " + name);
            return null;
        }

        private static int GetAllNetsCountFromTestData(List<PcbTestObject> resultData)
        {
            List<string> nets = new List<string>();
            foreach (var obj in resultData)
            {
                foreach (var pin in obj.Pins)
                {
                    foreach (var net in pin.Nets)
                    {
                        if (!nets.Contains(net))
                        {
                            nets.Add(net);
                        }
                    }
                }
            }

            return nets.Count;
        }
    }
}
