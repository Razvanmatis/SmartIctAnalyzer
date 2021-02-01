using GrpcApi.Handler;
using Interfaces.UnitTests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using PCBI.Automation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace TestProjectNet472
{
    [TestClass]
    public class TestPcbInvestigatorApi
    {
        private const string PATHTOODB = "C:\\Repositories\\smart_ict_analyser\\Testdaten\\panel";
        private static string filePath = Directory.GetCurrentDirectory() + "\\exportApiObjects.txt";

        [TestMethod]
        public void TestCollectingItemsFromApi()
        {            
            List<PcbTestObject> result = PcbInvestigatorApiHandler.GetAllObjectsFromPcbInvestigator(PATHTOODB);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
            SaveStorageContentIntoFile(result, filePath);
        }

        private static void SaveStorageContentIntoFile(List<PcbTestObject> content, string filePathToUse)
        {
            bool fileOk = true;
            if (!File.Exists(filePathToUse))
            {
                try
                {
                    File.Create(filePathToUse).Close();
                }
                catch (Exception e)
                {
                    fileOk = false;
                    Debug.WriteLine(e.Message);
                }
            }

            if (fileOk)
            {
                try
                {
                    string jsonString = JsonConvert.SerializeObject(content, Formatting.Indented);
                    File.WriteAllText(filePathToUse, jsonString);
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Error at serializing the settings object into JSON file: " + e.Message);
                }
            }
        }
    }
}
