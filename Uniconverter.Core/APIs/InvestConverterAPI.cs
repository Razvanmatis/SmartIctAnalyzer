using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Uniconverter.Core.DataModel;

namespace Uniconverter.Core.APIs
{
    internal class InvestConverterAPI
    {
        private string _pathtoPCBInvestConverter;
        private string _pathToTargetFile;
        public string _pathToSaveContent { get; }
        private List<FileContent> fileContents;


        public InvestConverterAPI(string PathtoPCBInvestConverter)
        {
            _pathtoPCBInvestConverter = PathtoPCBInvestConverter;

            
        }


        public List<FileContent> GetFileContent(string path)
        {
            _pathToTargetFile = path;
            GetFileContents();
            return fileContents;
        }


        private async void GetFileContents()
        {
            fileContents = new List<FileContent>();

            var result = await startReadoutProzess(_pathToTargetFile);

            int returnvalue = result.Item1;

            if(returnvalue == 6)
            {
                fileContents = result.Item2;
            }
            
        }


        private async Task<(int, List<FileContent>)>startReadoutProzess(string scriptFilePath)
        {
            
            string args = "-r " + "\""+scriptFilePath+"\"" + " auto";
            List<string> consoleOutput = new List<string>();
            string line;
            var FFileContents = new List<FileContent>();


            var PCPProzess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _pathtoPCBInvestConverter,
                    Arguments = args,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                }
            };

            PCPProzess.Start();
            while (!PCPProzess.StandardOutput.EndOfStream)
            {
                line = PCPProzess.StandardOutput.ReadLine();
                consoleOutput.Add(line);
            }
            PCPProzess.WaitForExit();
            int exitCode = PCPProzess.ExitCode;

            for (int i = 0; i < consoleOutput.Count; i++)
            {
                if (consoleOutput[i].Contains("#file#"))
                {
                    FileContent file = JsonConvert.DeserializeObject<FileContent>(consoleOutput[i+1]);
                    if(file != null)
                    {
                        FFileContents.Add(file);
                    }
                }
            }
            return (exitCode, FFileContents);
  
        }
    }

}
