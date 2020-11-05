using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Uniconverter.Core.DataModel;
using Uniconverter.Core.APIs;

namespace Uniconverter.Core.FileFormat
{
    internal class FileValidator
    {

        private string[] _fileContent;
        private string _fileName;
        private string _fileFormat;
        private string _filePath;
        private string _version;
        private string[] _validFileFormats = { "csv", "brd", "cad", "uni", "asc", "cc" , "uco" };
        private bool _filevalid = false;
        InvestConverterAPI iVestConverter;



        //private Conversions _conversion;

        public string[] FileContent => _fileContent;

        public string FileFormat => _fileFormat;

        public bool isValid => _filevalid;

        public string FilePath => _filePath;

        //public Conversions Einheit => _conversion;

        public string FileVersion => _version;

        public FileValidator(string filepath, string pathtoPCBInvestExe)
        {

            _filePath = filepath;

            iVestConverter = new InvestConverterAPI(pathtoPCBInvestExe);

            var u = iVestConverter.GetFileContent(filepath);

            FileInfo myFile = new FileInfo(_filePath);

            FileAttributes attr = File.GetAttributes(filepath);

            if (attr.HasFlag(FileAttributes.Directory))
            {
                _filevalid = this.checkifODB(myFile);
                _fileFormat = "tgz_odb";
            }
            else
            {
                _filePath = myFile.FullName;
                string[] dummy = myFile.FullName.Split("\\");
                _fileFormat = dummy[dummy.Length - 1].Split(".")[1];
                _fileName = dummy[dummy.Length - 1].Split(".")[0];
                for (int i = 0; i < _validFileFormats.Length; i++)
                {
                    if (_fileFormat == _validFileFormats[i])
                    {
                        _fileContent = System.IO.File.ReadAllLines(_filePath);
                    }
                }
                _filevalid = checkFileContentValidation();

            }

            _filevalid = checkFileContentValidation();

        }



        //public List<TestPoint> getTestPoints()
        //{
        //    if (!_filevalid)
        //    {
        //        return null;
        //    }

        //    switch (_fileFormat)
        //    {
        //        case "csv":
        //            return csvFileContent();
        //        case "uni":
        //            return getUniTP();
        //        case "tgz_odb":
        //            return tgzodbFileContent();
        //        case "brd":
        //            return brdFileContent();
        //        case "cad":
        //            return cadFileContent();
        //        case "asc":
        //            return ascFileContetn();
        //        case "cc":
        //            return cczFileContent();
        //        default:
        //            return null;
        //    }

        //}



        //private List<TestPoint> getUniTP()
        //{
        //    List<TestPoint> fileContent = new List<TestPoint>();

        //    for (int i = 0; i < _fileContent.Length; i++)
        //    {            
        //       ///Testpunkte einlesen

        //        if (_fileContent[i].Contains("%%TEST_NEEDLES") && _fileContent[i + 1] != String.Empty)
        //        {
        //            while (_fileContent[i] != String.Empty)
        //            {
        //                i++;
        //                string[] split = _fileContent[i].Split("|");
        //                string tpside = "";

        //                if (split[6] != String.Empty)
        //                {
        //                    tpside = "top";
        //                }
        //                else
        //                {
        //                    tpside = "bottom";
        //                }

        //                TestPoint tp = new TestPoint(split[0], split[2], stringtoDouble(split[4]), stringtoDouble(split[5]), tpside, split[3]);
        //                fileContent.Add(tp);
        //            }
        //        }            
        //    }

        //    return fileContent;
        //}

        //private List<Component> GetUniCP()
        //{
        //    List<Component> fileContent = new List<Component>();

        //    string CPonTopLayer, CPonBottomLayer;
        //    List<string> CPProperties, CPComponents;

        //    for (int i = 0; i < _fileContent.Length; i++)
        //    {
        //        if (_fileContent[i].Contains("TOP_LAYER"))
        //        {
        //            CPonTopLayer = _fileContent[i].Split("=")[1];
        //        }

        //        if (_fileContent[i].Contains("BOTTOM_LAYER"))
        //        {
        //            CPonBottomLayer = _fileContent[i].Split("=")[1];
        //        }



        //    }



        //}



        private FileContent tgzodbFileContent()
        {

            throw new NotImplementedException();
        }

        private FileContent brdFileContent()
        {
 
            throw new NotImplementedException();
        }

        private FileContent cadFileContent()
        {

            throw new NotImplementedException();
        }

        private FileContent ascFileContetn()
        {

            throw new NotImplementedException();
        }

        private FileContent cczFileContent()
        {

            throw new NotImplementedException();
        }

        private FileContent csvFileContent()
        {

            throw new NotImplementedException();
        }



        #region Private Helpers
        private bool checkifODB(FileInfo file)
        {

            string[] s = Directory.GetDirectories(file.FullName);

            if (s.Length > 0)
            {

                List<string> lists = new List<string>();

                for (int i = 0; i < s.Length; i++)
                {
                    string[] filepath = s[i].Split("\\");
                    lists.Add(filepath[filepath.Length - 1]);


                }

                if (lists.Contains("user") && lists.Contains("steps"))
                {
                    _fileFormat = "tgz_odb";
                    _filevalid = true;
                    return true;
                }

            }
            _fileFormat = "unknown";
            _filevalid = false;
            return false;
        }


        private bool checkFileContentValidation()
        {
            switch (_fileFormat)
            {
                case "csv":
                    return csvCheck();
                case "uni":
                    return uniCheck();
                case "tgz_odb":
                    return true;
                case "brd":
                    return brdCheck();
                case "cad":
                    return cadCheck();
                case "asc":
                    return ascCheck();
                case "cc":
                    return cczCheck();
                default:
                    return false;
            }

        }

        private bool csvCheck()
        {
            return false;
        }

        private bool uniCheck()
        {
            if (_fileContent[1].Contains("UNIDAT"))
            {
                _version = _fileContent[1].Split("=")[1];

                /// Verwendete Einheit einlesen
                if (_fileContent[4].Contains("UNITS"))
                {
                    if (_fileContent[4].Contains("MM"))
                    {
                       //_conversion = Conversions.mm;
                    }
                    else
                    {
                        //_conversion = Conversions.unknown;
                    }
                }

                return true;
            }
            return false;
        }

        private bool brdCheck()
        {
            if (_fileContent[1].Contains("BOARD_FILE"))
            {
                _version = _fileContent[1].Split("   ")[1];
                return true;
            }
            return false;
        }

        private bool cadCheck()
        {
            if (_fileContent[0].Contains("R!version"))
            {
                _version = _fileContent[0].Split("!")[1].Split(" ")[1];
                return true;
            }
            return false;
        }

        private bool ascCheck()
        {
            if (_fileContent[0].Contains("!PADS-POWERPCB"))
            {
                _version = "No Versions Info";
                return true;
            }
            return false;
        }
        private bool cczCheck()
        {
            if (_fileContent[1].Contains("CCDoc"))
            {
                string[] split = _fileContent[1].Split(" ");

                for (int i = 0; i < split.Length; i++)
                {
                    if (split[i].Contains("version"))
                    {
                        string[] version = split[i].Split("=");
                        _version = version[1];
                    }
                }

            }

            return false;
        }

        private double stringtoDouble(string value)
        {
            double returnvalue;
            bool convertok;

            if (value != null)
            {

                convertok = double.TryParse(value, System.Globalization.NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out returnvalue);
                if (convertok)
                {
                    return Math.Round(returnvalue, 2);
                }
                return 0.00;

            }

            return 0.00;
        }



        #endregion



    }
}
