using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PCBInvestAPI;
using PCBInvestAPI.DataModel.OutLineObjects;
using PCBI;
using PCBInvestAPI.Exports;
using PCBInvestAPI.ReadInConfigurations;
using Newtonsoft.Json;
using PCBInvestAPI.DataModel;
using PCBInvestAPI.Information;
using System.IO;
using PCBInvestAPI.CCZHandling;


namespace PCBInvestConverter
{
    class Program
    {
        [STAThread]
        static int Main(string[] args)
        {
            args = new string[] { "-r", "C:\\Users\\rama\\Berufsbegleitender Masterstudiengang für Software-Engineering und " +
                "Informationstechnik\\Master-Thesis\\Smart ICT analysis Tool\\uniconverter_2\\PCBInvestConverter\\bin\\Debug\\daten\\" +
                "valor_odb", "-f","xlsx"};
            ApplicationSettings _appSettings;
            List<Infos> _consoleOutput = new List<Infos>();
            
            string _PathtoFile = "";
            string _ExportFileName = "";
            string _ExportFilePath = "";
            string _ExportFileFormat = "";
            string _OutOutlinesforselectedcMPs = "";
            bool _startReadoutInfos = false;
            bool onlyConsoleJsonOutput = false;

            if (args.Length == 0)
            {
                Infos info = new Infos(Info.notFound, InfoType.applicationsettings, "Follow Arguments requierd for export datas \n" +
                                                                                      "-r <PathtoFile> File to read in \n" +
                                                                                      "-e <exportFileName> default = NoName \n" +
                                                                                      "-p <exportPath> default = PathofFile \n" +
                                                                                      "-------------------------------------------------");


                Infos info1 = new Infos(Info.notFound, InfoType.applicationsettings, "Follow Arguments possible \n" +
                                                                                      "-r <PathtoFile> File to read in \n" +
                                                                                      "-i <tp,cp,step,nets> Informations about Testpoints, or Components or Steps or Nest" +
                                                                                      "------------------------------------------------------------------------------------");


                Infos info2 = new Infos(Info.ok, InfoType.applicationsettings, "No arguments found");
                Infos info3 = new Infos(Info.ok, InfoType.applicationsettings, "Start selfckeck");

                _consoleOutput.Add(info);
                _consoleOutput.Add(info1);
                _consoleOutput.Add(info2);
                _consoleOutput.Add(info3);

                InitApp(out _appSettings, ref _consoleOutput);


                foreach (Infos item in _consoleOutput)
                {
                    item.writeInfotoConsole();
                }
                return (int)ReturnValue.CouldnnotReadSettings;
            }


            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-r")
                {
                    _PathtoFile = args[i + 1];
                }
                if (args[i] == "-e")
                {
                    _ExportFileName = args[i + 1];
                }
                if (args[i] == "-p")
                {
                    _ExportFilePath = args[i + 1];
                }
                if (args[i] == "-f")
                {
                    if(args[i+1] != null && args[i+1] == "xlsx")
                    {
                        _ExportFileFormat = "xlsx";
                    }
                    if (args[i + 1] != null && args[i + 1] == "csv")
                    {
                        _ExportFileFormat = "csv";
                    }
                    if (args[i + 1] != null && args[i + 1] == "uco")
                    {
                        _ExportFileFormat = "uco";
                    }
                }
                if (args[i] == "-i")
                {
                    _startReadoutInfos = true;
                }

                if(args[i] == "-o")
                {
                    _OutOutlinesforselectedcMPs = args[i + 1];
                }

                if(args[i] == "auto"){
                    onlyConsoleJsonOutput = true;
                }
            }

            if (!Directory.Exists(_PathtoFile))
            {
                Infos In = new Infos(Info.error, InfoType.applicationsettings, "Path not Exist "+ _PathtoFile);
                _consoleOutput.Add(In);
                showAllInfoMessages(_consoleOutput);
                return (int)ReturnValue.withError;
            }

            if(_ExportFileName == String.Empty)
            {
                _ExportFileName = "NoName";
            }

            if (_ExportFilePath == String.Empty)
            {
                if (File.Exists(_PathtoFile))
                {

                    string[] r = _PathtoFile.Split('\\');

                    List<string> list = new List<string>(r);
                    list.RemoveAt(r.Length - 1);

                    string[] newR = list.ToArray();

                    _ExportFilePath = String.Join("\\",newR);

                }
                else
                {
                    _ExportFilePath = _PathtoFile;
                }
            }
 

            if (!InitApp(out _appSettings, ref _consoleOutput))
            {

                foreach (Infos i in _consoleOutput)
                {
                    i.writeInfotoConsole();
                }
                return (int)ReturnValue.CouldnnotReadSettings;
            }

            if (!_appSettings.PCBInvestigatorAvailable)
            {
                Infos infos = new Infos(Info.notFound, InfoType.applicationsettings, "No PCBInvestigator founded");
                _consoleOutput.Add(infos);
                foreach (Infos i in _consoleOutput)
                {
                    i.writeInfotoConsole();
                }
                return (int)ReturnValue.PCBIsNotAvailable;
            }


            Console.WriteLine("Start Application");


            ///Start Readout PCB

            PcbAPI pcbAPI = new PcbAPI(_PathtoFile, _appSettings);


            if (pcbAPI.getInFoFromPCBInvestigator != 1)
            {
                string infoMessage;
                switch (pcbAPI.getInFoFromPCBInvestigator)
                {
                    case 0:
                        infoMessage = "PCB-Investigator:" + "Unkown";
                        break;
                    case 2:
                        infoMessage = "PCB-Investigator:" + "File not found, because path is unknown.";
                        break;
                    case 3:
                        infoMessage = "PCB-Investigator:" + "The relevant plugin.dll is not available, you can check the install directory PCB-Investigator/PlugIn for the relevant dll.";
                        break;
                    case 4:
                        infoMessage = "PCB-Investigator:" + "There was an error, please check errorlog for details.";
                        break;
                    case 5:
                        infoMessage = "PCB-Investigator:" + "With the current license no right to open the selected format. ";
                        break;
                    default:
                        infoMessage = "PCB-Investigator:" + "PCB Investigator FehlerCode unbekannt";
                        break;
                }

                Infos infos = new Infos(Info.error, InfoType.applicationsettings, infoMessage);
                _consoleOutput.Add(infos);
                showAllInfoMessages(_consoleOutput);
                return (int)ReturnValue.PCBIsNotAvailable;
            }

            List<string> steps = new List<string>();
            steps = pcbAPI.getAllSteps();




            if (steps == null || steps.Count == 0)
            {
                Infos infos = new Infos(Info.notFound, InfoType.steps, "No steps found for" + _PathtoFile);
                _consoleOutput.Add(infos);

                showAllInfoMessages(_consoleOutput);

                return (int)ReturnValue.withError;

            }

            FileInformations fileInfos;

            _startReadoutInfos = true;

            if (_startReadoutInfos)
            {
                fileInfos = new FileInformations();

                fileInfos = pcbAPI.ODBFileInfo;

                CSVExport cSVExport = new CSVExport();
                cSVExport.startExportFileInfos(_ExportFilePath, _ExportFileName, fileInfos);
            }

            if(_OutOutlinesforselectedcMPs != String.Empty)
            {
                if (_OutOutlinesforselectedcMPs == "all")
                {
                    List<string> allCMPNames = pcbAPI.getAllNamesfromComponents();

                    for (int i = 0; i < allCMPNames.Count; i++)
                    {
                        List<BaseOutlineObject> outlineObjects = new List<BaseOutlineObject>();

                        outlineObjects = pcbAPI.getOutlinefromComponent(allCMPNames[i]);

                        if (outlineObjects != null || outlineObjects.Count == 0)
                        {
                            CSVExport cSV = new CSVExport();
                            cSV.startExportComponentenOutlineinCSV(_ExportFilePath, _ExportFileName, ref _consoleOutput, outlineObjects, allCMPNames[i]);
                        }

                    }

                }
                else
                {
                    string[] comps = _OutOutlinesforselectedcMPs.Split(',');

                    for (int i = 0; i < comps.Length; i++)
                    {
                        List<BaseOutlineObject> outlineObjects = new List<BaseOutlineObject>();

                        outlineObjects = pcbAPI.getOutlinefromComponent(comps[i]);

                        if (outlineObjects != null || outlineObjects.Count == 0)
                        {
                            CSVExport cSV = new CSVExport();
                            cSV.startExportComponentenOutlineinCSV(_ExportFilePath, _ExportFileName, ref _consoleOutput, outlineObjects, comps[i]);
                        }

                    }


                }



            }



            List<List<BaseOutlineObject>> allOutlines = new List<List<BaseOutlineObject>>();

            ExportHandler ExcelAPI = null;
            CSVExport CSVExport = null;

            if (_appSettings.ExcelWorks && (_ExportFileFormat == "xlsx" || _ExportFileFormat == "") && !onlyConsoleJsonOutput)
            {
                ExcelAPI = new ExportHandler();
            }
            else if(!onlyConsoleJsonOutput && _ExportFileFormat=="csv")
            {
                CSVExport = new CSVExport();
            }



            List<FileContent> JsonConsoleOutput = new List<FileContent>();

            for (int i = 0; i < steps.Count; i++)
            {

                FileContent fileContent = new FileContent();
                fileContent.FileName = steps[i];
                fileContent.FileFormat = "tgz_odb";

                List<TestPoint> tpListe;
                List<Component> cpListe;
                List<NetList> netLite;
                List<BaseOutlineObject> outline = new List<BaseOutlineObject>();

                pcbAPI.readouComponents(i, out tpListe, out cpListe);
                pcbAPI.readoutNestInformations(i, out netLite, ref tpListe, ref cpListe);

                outline = pcbAPI.getPCBOutline(i);
                fileContent.PCBOutline = outline;

                fileContent.TestPoints = tpListe;
                fileContent.Componenten = cpListe;

                JsonConsoleOutput.Add(fileContent);

                allOutlines.Add(outline);

                if (ExcelAPI != null)
                {
                    ExcelAPI.startExport(_ExportFilePath, _ExportFileName + steps[i],ref _consoleOutput ,tpListe, cpListe, netLite);
                }
                else if(CSVExport != null)
                {
                    CSVExport.startExportCSV(_ExportFilePath, _ExportFileName + steps[i], ref _consoleOutput, tpListe, cpListe, netLite) ;
                }              
            }

            if (ExcelAPI != null)
            {
                ExcelAPI.startExportOutline(_ExportFilePath, _ExportFileName, ref _consoleOutput, allOutlines, steps);
            }
            else if(CSVExport != null)          
            {
                CSVExport.startExportPCBOutlineinCSV(_ExportFilePath, _ExportFileName, ref _consoleOutput, allOutlines, steps);
            }


            if (_ExportFileFormat == "uco")
            {
                for (int i = 0; i < JsonConsoleOutput.Count; i++)
                {
                    var exportpath = "";

                    if(_ExportFilePath != String.Empty)
                    {
                        exportpath = _ExportFilePath + "\\" + JsonConsoleOutput[i].FileName + ".uco";
                    }
                    else
                    {
                        exportpath = _PathtoFile + "\\" + JsonConsoleOutput[i].FileName + ".uco";
                    }

                    string jsonstring = JsonConvert.SerializeObject(JsonConsoleOutput[i], Formatting.Indented);
                    using (StreamWriter file = File.CreateText(exportpath))
                    {
                        file.Write(jsonstring);
                    }
                }
            }



            if (onlyConsoleJsonOutput)
            {
                for (int i = 0; i < JsonConsoleOutput.Count; i++)
                {
                    Console.WriteLine("#file#");
                    Console.WriteLine(JsonConvert.SerializeObject(JsonConsoleOutput[i]));                  
                }


                pcbAPI.closePCB();
                return (int)ReturnValue.ConsoloutputJson;
            }

            
            Console.WriteLine("Finish");
            showAllInfoMessages(_consoleOutput);

            pcbAPI.closePCB();
            
            if(ExcelAPI != null)
            {
                ExcelAPI.closeExcel();
                return (int)ReturnValue.Successful_as_xlsx;
            }
            if(CSVExport != null)
            {
                return (int)ReturnValue.Successful_as_csv;
            }
            return (int)ReturnValue.withError;
        }


        public static bool InitApp(out ApplicationSettings settings, ref List<Infos> appInfos)
        {
            Console.WriteLine("Read AppSettings");
            ApplicationSettings set = new ApplicationSettings();
            ReadInConfigurations createAppSettings = new ReadInConfigurations();
            bool updateSettings = false;


            if (!createAppSettings.FoundAppSettings)
            {

                Console.WriteLine("Please control the settings and restart the application");
                Infos info = new Infos(Info.error, InfoType.applicationsettings, "MainAppSettings not found");
                appInfos.Add(info);
                settings = set;
                return false;
            }
            Infos Info2 = new Infos(Info.ok, InfoType.applicationsettings, "MainAppSettings ok");
            appInfos.Add(Info2);
            set = createAppSettings.Settings;
            settings = set;

            if (!settings.PCBInvestigatorAvailable)
            {
                var pathtoPCB = settings.PathtoPCBExe;
                string[] path = pathtoPCB.Split('\\');
                if (path[path.Length - 1] != "PCB - Investigator.exe")
                {
                    Infos infos = new Infos(Info.notFound, InfoType.applicationsettings, "Did not find the right Path to PCB - Investigator.exe - Actualpath is : " + pathtoPCB);
                    appInfos.Add(infos);
                }

                PcbAPI pcbAPITest = new PcbAPI();
                if (pcbAPITest.tryPCBisRunning(pathtoPCB))
                {
                    settings.PCBInvestigatorAvailable = true;
                    updateSettings = true;
                    Infos infos = new Infos(Info.ok, InfoType.applicationsettings, "PCB API works \n" + "ApplicationSettings will be updatet ");
                    appInfos.Add(infos);
                }

            }

            if (updateSettings)
            {
                createAppSettings.UpdateSettings(settings);
                createAppSettings = new ReadInConfigurations();
                settings = createAppSettings.Settings;

            }


            //

            return true;
        }

        public static void showAllInfoMessages(List<Infos> infos)
        {
            foreach (Infos item in infos)
            {
                item.writeInfotoConsole();
            }
        }

        public static string AddQuotesIfRequired(string path)
        {
            return !string.IsNullOrWhiteSpace(path) ?
                path.Contains(" ") && (!path.StartsWith("\"") && !path.EndsWith("\"")) ?
                    "\"" + path + "\"" : path :
                    string.Empty;
        }
    }
}
