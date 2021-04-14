using GrpcApi.Implementations;
using Interfaces.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace TestProjectNet472
{
    [TestClass]
    public class TestPcbInvestigatorApi
    {
        private const string PATHTOODB = "C:\\Repositories\\smart_ict_analyser\\Testdaten\\panel";
        private static readonly string FILEPATH = Directory.GetCurrentDirectory() + "\\exportApiObjects.txt";

        [TestMethod]
        public void TestCollectingItemsFromApi()
        {            
            List<PcbTestObject> result = PcbInvestigatorApiHandler.GetAllObjectsFromPcbInvestigator(PATHTOODB);
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
            SaveStorageContentIntoFile(result, FILEPATH);
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
