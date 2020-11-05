using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using Uniconverter.Core.DataModel;
using Uniconverter.Core.APIs;
using Uniconverter.Core;
using Uniconverter.Core.FileFormat;


namespace Uniconverter.Core
{
    public class FileHandler
    {   
        
        #region Private 
        //private BaseFile _loadedFile;
        private ObservableCollection<TestPoint> _activList;
        private string _pathToPCBInvestConverterExe;
        private List<FileContent> _loadedFiles;
       

        #endregion


        #region Public


        #endregion

        public FileHandler(string PathtoPCBInvestConverter)
        {

            _pathToPCBInvestConverterExe = PathtoPCBInvestConverter;
            
        }


        public bool loadFile(string PathtoFile, ref bool startReadInProjectName , string fileName ="", string exportPath ="")
        {

            FileValidator fileValidator = new FileValidator(PathtoFile, _pathToPCBInvestConverterExe);

            string NewFileName;
                       
            string[] split = PathtoFile.Split("\\");


            if (fileName != String.Empty)
            {
                NewFileName = fileName;
            }
            else
            {
                NewFileName = split[split.Length - 1];
            }


            if(_loadedFiles != null)
            {
                foreach (FileContent item in _loadedFiles)
                {
                    if(item.FileName == NewFileName)
                    {
                        startReadInProjectName = true;
                        return false;
                    }
                }
            }

            FileConfiguration newFileConifg = new FileConfiguration
            {
                FilePath = PathtoFile,
                PathtoPCBInvestConverter = _pathToPCBInvestConverterExe,
                FileName = NewFileName,
                ExportPath = exportPath
            };

            FileContent newFile = new FileContent();



            if (_loadedFiles == null)
            {
                _loadedFiles = new List<FileContent>();
            }

            _loadedFiles.Add(newFile);

            return true;
        }

    }


    public struct FileConfiguration
    {
        public string PathtoPCBInvestConverter { get; set; }
        public string FilePath { get; set; }
        public string ExportPath { get; set;}
        public string FileName { get; set; }
    }
}
