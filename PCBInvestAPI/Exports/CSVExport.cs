using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using PCBInvestAPI.DataModel;
using PCBInvestAPI.DataModel.OutLineObjects;
using PCBInvestAPI.Information;

namespace PCBInvestAPI.Exports
{

    public class CSVExport
    {

        public bool startExportCSV(string path, string fileName, ref List<Infos> consoleOutPut, List<TestPoint> listTP, List<Component> listCP = null, List<NetList> listNet = null)
        {

            Console.WriteLine("Start creating csv-Files");


            string completFileNameTestpoints = FileNameGenerator(path, fileName, "TP", "", ".csv");
            string completFileNameComponents = FileNameGenerator(path, fileName, "CP", "", ".csv");
            string completFileNameNets = FileNameGenerator(path, fileName, "Nets", "", ".csv");

            int i = 1;
            while (File.Exists(completFileNameTestpoints) || File.Exists(completFileNameComponents) || File.Exists(completFileNameNets))
            {
                completFileNameTestpoints = FileNameGenerator(path, fileName, "TP", "_" + i.ToString(), ".csv");
                completFileNameComponents = FileNameGenerator(path, fileName, "CP", "_" + i.ToString(), ".csv");
                completFileNameNets = FileNameGenerator(path, fileName, "Nets", "_" + i.ToString(), ".csv");

                i++;
            }


            if (listTP != null)
            {
                consoleOutPut.Add(new Infos(Info.ok, InfoType.tp, "found"));
                using (StreamWriter file = File.CreateText(completFileNameTestpoints))
                {
                    file.WriteLine("TPName;TPDescription;X-Value;Y-Value;Position;ProgrammingPoint;NetList");
                    foreach (TestPoint tp in listTP)
                    {
                        file.WriteLine(tp.TPname + ";"
                                      + tp.TPDescription + ";"
                                      + tp.X_Value + ";"
                                      + tp.Y_Value + ";"
                                      + tp.PointPos + ";"
                                      + tp.ProgrammingPoint.ToString() + ";"
                                      + tp.NetList);
                    }
                }
            }
            else
            {
                Console.WriteLine("No TestPoints found");
            }


            if (listCP != null)
            {
                consoleOutPut.Add(new Infos(Info.ok, InfoType.comps, "found"));
                using (StreamWriter file = File.CreateText(completFileNameComponents))
                {
                    file.WriteLine("CPName;CPDescription;X-Value;Y-Value;Position;CPHigh;NetList");
                    foreach (Component cp in listCP)
                    {
                        file.WriteLine(cp.CPName + ";"
                                      + cp.CPDescription + ";"
                                      + cp.X_Value + ";"
                                      + cp.Y_Value + ";"
                                      + cp.CPPosition + ";"
                                      + cp.CPHigh.ToString() + ";"
                                      + cp.NetList);
                    }
                }
            }

            if (listNet != null)
            {
                consoleOutPut.Add(new Infos(Info.ok, InfoType.nets, "found"));
                using (StreamWriter file = File.CreateText(completFileNameNets))
                {
                    foreach (NetList net in listNet)
                    {

                        string liste = net.NetListName + ";";
                        for (int u = 0; u < net.Teilnehmer.Count; u++)
                        {
                            liste += net.Teilnehmer[u] + ";";
                        }

                        file.WriteLine(liste);
                    }
                }
            }
            else
            {
                Console.WriteLine("No Components found");
            }

            if (File.Exists(completFileNameTestpoints))
            {
                Console.WriteLine("File " + completFileNameTestpoints + "generated");
                consoleOutPut.Add(new Infos(Info.ok, InfoType.tp, "File ok", completFileNameTestpoints));
            }
            else
            {
                Console.WriteLine("Could not generat File " + completFileNameTestpoints);
                consoleOutPut.Add(new Infos(Info.notFound, InfoType.tp, "File NIO", completFileNameTestpoints));
            }


            if (File.Exists(completFileNameComponents))
            {
                Console.WriteLine("File " + completFileNameComponents + "generated");
                consoleOutPut.Add(new Infos(Info.ok, InfoType.comps, "File ok", completFileNameComponents));
            }
            else
            {
                Console.WriteLine("Could not generat File " + completFileNameComponents);
                consoleOutPut.Add(new Infos(Info.notFound, InfoType.comps, "File NIO", completFileNameComponents));
            }

            if (File.Exists(completFileNameNets))
            {
                Console.WriteLine("File " + completFileNameNets + "generated");
                consoleOutPut.Add(new Infos(Info.ok, InfoType.nets, "File ok", completFileNameNets));
            }
            else
            {
                Console.WriteLine("Could not generat File " + completFileNameNets);
                consoleOutPut.Add(new Infos(Info.notFound, InfoType.nets, "File NIO", completFileNameNets));
            }

            return true;
        }


        public bool startExportPCBOutlineinCSV(string path, string fileName, ref List<Infos> consoleOutPut, List<List<BaseOutlineObject>> outlinecmp, List<string> steps)
        {
            Console.WriteLine("Start Export CSV Outline Datas");

            List<string> fileListe = new List<string>();


            for (int i = 0; i < steps.Count; i++)
            {
                string completFileNameExcel = FileNameGenerator(path, fileName + steps[i], "Outlines", "", ".csv");
                fileListe.Add(completFileNameExcel);
            }


            ///Create the CSV Output for all are outlines form all steps

            List<List<string>> CSVOutput = new List<List<string>>();

            if(outlinecmp == null || outlinecmp.Count == 0)
            {
                consoleOutPut.Add(new Infos(Info.notFound, InfoType.pcbOutline, "NIO"));
                return false;
            }

            consoleOutPut.Add(new Infos(Info.ok, InfoType.pcbOutline, "found"));

            foreach (List<BaseOutlineObject> outline in outlinecmp)
            {

                List<string> csvData = new List<string>();

                for (int u = 0; u < outline.Count; u++)
                {
                    string zeile = "";

                    if (outline[u].ToString() == "PCBInvestAPI.DataModel.OutLineObjects.ArcOutlineObject")
                    {
                        var arc = (ArcOutlineObject)outline[u];

                        zeile = "Arc" + ";"
                                + arc.X_Center + ";"
                                + arc.Y_Center + ";"
                                + arc.X_StartPoint + ";"
                                + arc.Y_StartPoint + ";"
                                + arc.X_EndPoint + ";"
                                + arc.Y_EndPoint + ";"
                                + arc.Diameter + ";"
                                + arc.Clockwise;

                    }
                    if (outline[u].ToString() == "PCBInvestAPI.DataModel.OutLineObjects.LineOutlineObject")
                    {
                        var line = (LineOutlineObject)outline[u];

                        zeile = "Line" + ";"
                            + line.X_StartPoint + ";"
                            + line.Y_StartPoint + ";"
                            + line.X_EndPoint + ";"
                            + line.Y_EndPoint;
                    }

                    csvData.Add(zeile);
                }

                CSVOutput.Add(csvData);
            }

            for (int i = 0; i < fileListe.Count; i++)
            {
                List<string> csvwrite = CSVOutput[i];
                using (StreamWriter file = File.CreateText(fileListe[i]))
                {

                    for (int u  = 0; u < csvwrite.Count; u++)
                    {
                        file.WriteLine(csvwrite[u]);
                    }                 
                }
            }

            for (int i = 0; i < fileListe.Count; i++)
            {
                if (File.Exists(fileListe[i]))
                {
                    Console.WriteLine("Successful created {0}", fileListe[i]);
                    consoleOutPut.Add(new Infos(Info.ok, InfoType.pcbOutline, "ok",fileListe[i]));
                }
                else
                {
                    Console.WriteLine("Could not create {0}", fileListe[i]);
                    consoleOutPut.Add(new Infos(Info.notFound, InfoType.pcbOutline, "NIO",fileListe[i]));
                }
            }


            return true;
        }

        public bool startExportComponentenOutlineinCSV(string path, string fileName,ref List<Infos> consoleOutPut, List<BaseOutlineObject> outlinecmp, string compName)
        {
            Console.WriteLine("Start Export CSV ComponentenOutline");

            var PathforOutlines = "\\Outlines";

            string completFileNameExcel = FileNameGenerator(PathforOutlines, fileName + "_" + compName, "_outline", "", ".csv");

            ///Create the CSV Output for all are outlines form all steps


            using (StreamWriter file = File.CreateText(completFileNameExcel))
            {
                foreach (BaseOutlineObject outline in outlinecmp)
                {

                    List<string> csvData = new List<string>();

                    string zeile = "";

                    if (outline.ToString() == "PCBInvestAPI.DataModel.OutLineObjects.ArcOutlineObject")
                    {
                        var arc = (ArcOutlineObject)outline;

                        zeile = "Arc" + ";"
                                + arc.X_Center + ";"
                                + arc.Y_Center + ";"
                                + arc.X_StartPoint + ";"
                                + arc.Y_StartPoint + ";"
                                + arc.X_EndPoint + ";"
                                + arc.Y_EndPoint + ";"
                                + arc.Diameter + ";"
                                + arc.Clockwise;

                    }
                    if (outline.ToString() == "PCBInvestAPI.DataModel.OutLineObjects.LineOutlineObject")
                    {
                        var line = (LineOutlineObject)outline;

                        zeile = "Line" + ";"
                            + line.X_StartPoint + ";"
                            + line.Y_StartPoint + ";"
                            + line.X_EndPoint + ";"
                            + line.Y_EndPoint;
                    }

                    file.WriteLine(zeile);

                }

            }



                if (File.Exists(completFileNameExcel))
                {
                    Console.WriteLine("Successful created {0}", completFileNameExcel);
                    consoleOutPut.Add(new Infos(Info.ok, InfoType.pcbOutline, "ok", completFileNameExcel));
                return true;
                }
                else
                {
                    Console.WriteLine("Could not create {0}", completFileNameExcel);
                    consoleOutPut.Add(new Infos(Info.notFound, InfoType.pcbOutline, "NIO", completFileNameExcel));
                return false;
                }
            


          
        }

        public void startExportFileInfos(string path, string fileName, FileInformations exportInfos)
        {
            string FileInformationsPath = FileNameGenerator(path, fileName, "info", "", ".csv");

            int i = 1;
            while (File.Exists(FileInformationsPath))
            {

                FileInformationsPath = FileNameGenerator(path, fileName, "info", "_" +i.ToString(), ".csv");

                i++;
            }

            using (StreamWriter file = File.CreateText(FileInformationsPath))
            {
                file.WriteLine("StepName;LayerName;ObjectCount");

                string line = "";
                for (int u = 0; u < exportInfos.ListofLayers.Count; u++)
                {
                    line = exportInfos.ListofLayers[u].StepName + ";" 
                         + exportInfos.ListofLayers[u].LayerName + ";" 
                         + exportInfos.ListofLayers[u].ObjectCount;

                    file.WriteLine(line);
                }
              
            }


        }

        private string FileNameGenerator(string Path, string FileName, string FileNameAnhang, string endName, string format)
        {
            return Path + "\\" + FileName + "_" + FileNameAnhang + endName + format;
        }


    }
}
