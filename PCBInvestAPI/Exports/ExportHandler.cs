using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Excel = Microsoft.Office.Interop.Excel;
using Microsoft.CSharp;
using PCBInvestAPI.DataModel;
using PCBInvestAPI.DataModel.OutLineObjects;
using PCBInvestAPI.Information;

namespace PCBInvestAPI.Exports
{
    public class ExportHandler
    {
        private Excel.Application xlApp;


        public ExportHandler()
        {
            xlApp = new Excel.Application();
        }


        public bool startExport(string path, string fileName,ref List<Infos> consoleOutPut, List<TestPoint> listTP, List<Component> listCP = null , List<NetList> listNet= null  )
        {


            if (!Directory.Exists(path))
            {
                Console.WriteLine("Exportpath not exist");
                return false;
            }

            /// Prüfung ob eine Verbindung zur ExcelAPI möglich ist.
                       
                Console.WriteLine("Start creating ExcelFile - xlsx");

                string completFileNameExcel = FileNameGenerator(path, fileName, "TP_CP", "", ".xlsx");


                int i = 1;
                while (File.Exists(completFileNameExcel))
                {
                   completFileNameExcel = FileNameGenerator(path, fileName, "TP_CP","_"+i.ToString(), ".xlsx");
                   i++;
                }

                var Workbook = xlApp.Workbooks.Add();


                if (listTP != null && listTP.Count > 0)
                {

                    var sheet = (Excel.Worksheet)Workbook.Worksheets.Add();
                    this.TPcreateWorkSheet(ref sheet, listTP);
                    consoleOutPut.Add(new Infos(Info.ok, InfoType.tp, "found"));
                }
                if(listCP != null && listCP.Count > 0)
                {
                    var sheet = (Excel.Worksheet)Workbook.Worksheets.Add();
                    this.CompocreateWorkSheet(ref sheet, listCP);
                    consoleOutPut.Add(new Infos(Info.ok, InfoType.comps, "found"));
                }
                if (listNet != null && listNet.Count > 0)
                {
                    var sheet = (Excel.Worksheet)Workbook.Worksheets.Add();

                    this.NetListcreateWorkSheet(ref sheet, listNet);
                    consoleOutPut.Add(new Infos(Info.ok, InfoType.nets, "found"));
                }



                Workbook.SaveAs(completFileNameExcel);
                Workbook.Close(false,completFileNameExcel);

                if (File.Exists(completFileNameExcel))
                {
                    Console.WriteLine("File " + completFileNameExcel + "generated");
                    consoleOutPut.Add(new Infos(Info.ok, InfoType.comps, "ok",completFileNameExcel));
                }
                else
                {
                    Console.WriteLine("Could not generat File " + completFileNameExcel);
                    consoleOutPut.Add(new Infos(Info.notFound, InfoType.comps, "NIO", completFileNameExcel));
            }



                return true;
        }

        public bool startExportOutline(string path, string fileName, ref List<Infos> consoleOutPut, List<List<BaseOutlineObject>> outlinecmp , List<string> steps)
        {
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Exportpath not exist");
                return false;
            }
           
                Console.WriteLine("Start creating ExcelFile for Outline - xlsx");

                string completFileNameExcel = FileNameGenerator(path, fileName, "Outlines", "", ".xlsx");


                int i = 1;
                while (File.Exists(completFileNameExcel))
                {
                    completFileNameExcel = FileNameGenerator(path, fileName, "Outlines", "_" + i.ToString(), ".xlsx");
                    i++;
                }

            if (outlinecmp != null && outlinecmp.Count > 0)
            {
                consoleOutPut.Add(new Infos(Info.ok, InfoType.pcbOutline, "found"));
                var Workbook = xlApp.Workbooks.Add();

                for (int z = 0; z < outlinecmp.Count; z++)
                {
                    var sheet = (Excel.Worksheet)Workbook.Worksheets.Add();

                    this.CreateOutLineWorkSheet(ref sheet, steps[z], outlinecmp[z]);

                }
                Workbook.SaveAs(completFileNameExcel);
                Workbook.Close(false, completFileNameExcel);
            }
            else
            {
                consoleOutPut.Add(new Infos(Info.notFound, InfoType.pcbOutline, "File NIO"));
            }


                if (File.Exists(completFileNameExcel))
                {   
                    Console.WriteLine("File " + completFileNameExcel + "generated");
                    consoleOutPut.Add(new Infos(Info.ok, InfoType.pcbOutline, "File ok",completFileNameExcel));    
                }
                else
                {
                    Console.WriteLine("Could not generat File " + completFileNameExcel);
                    consoleOutPut.Add(new Infos(Info.notFound, InfoType.pcbOutline, "File NIO", completFileNameExcel));
                }

            return true;
        }
               
        public void closeExcel()
        {
            if(xlApp != null)
            {
                xlApp.Quit();
                xlApp = null;
            }
        }

        private void TPcreateWorkSheet(ref Excel.Worksheet sheet, List<TestPoint> testPoints)
        {
            
            sheet.Name = "TestPoints";

            //TPName;TPDescription;X-Value;Y-Value;Position;ProgrammingPoint

            sheet.Cells[1, 1] = "TPName";
            sheet.Cells[1, 2] = "TPDescription";
            sheet.Cells[1, 3] = "X-Value";
            sheet.Cells[1, 4] = "Y-Value";
            sheet.Cells[1, 5] = "Position";
            sheet.Cells[1, 6] = "ProgrammingPoint";
            sheet.Cells[1, 7] = "NetList";


            for (int i = 0; i < testPoints.Count; i++)
            {
                sheet.Cells[i + 2, 1] = testPoints[i].TPname;
                sheet.Cells[i + 2, 2] = testPoints[i].TPDescription;
                sheet.Cells[i + 2, 3] = testPoints[i].X_Value;
                sheet.Cells[i + 2, 4] = testPoints[i].Y_Value;
                sheet.Cells[i + 2, 5] = testPoints[i].PointPos;
                sheet.Cells[i + 2, 6] = testPoints[i].ProgrammingPoint.ToString();
                sheet.Cells[i + 2, 7] = testPoints[i].NetList;
            }

            for (int i = 1; i < 8; i++)
            {
                Excel.Range formated = sheet.Range[InttoExcelIndex(1, i), InttoExcelIndex(testPoints.Count, i)];
                formated.Columns.AutoFit();

            }

        }

        private void CompocreateWorkSheet(ref Excel.Worksheet sheet, List<Component> components)
        {
            

            sheet.Name = "Components";

            //CPName;CPDescription;X-Value;Y-Value;Position;CPHigh

            sheet.Cells[1, 1] = "CPName";
            sheet.Cells[1, 2] = "CPDescription";
            sheet.Cells[1, 3] = "X-Value";
            sheet.Cells[1, 4] = "Y-Value";
            sheet.Cells[1, 5] = "Position";
            sheet.Cells[1, 6] = "CPHigh";
            sheet.Cells[1, 7] = "NetList";

            for (int i = 0; i < components.Count; i++)
            {
                sheet.Cells[i + 2, 1] = components[i].CPName;
                sheet.Cells[i + 2, 2] = components[i].CPDescription;
                sheet.Cells[i + 2, 3] = components[i].X_Value;
                sheet.Cells[i + 2, 4] = components[i].Y_Value;
                sheet.Cells[i + 2, 5] = components[i].CPPosition;
                sheet.Cells[i + 2, 6] = components[i].CPHigh;
                sheet.Cells[i + 2, 7] = components[i].NetList;
            }


            ///Es müssen alle Zeilen einer Spalte, die Informationen beinhalten in ein Range eingefügt werden um AutoFit korrekt ausführen zu können.
            for (int i = 1; i < 8; i++)
            {
                Excel.Range formated = sheet.Range[InttoExcelIndex(1, i), InttoExcelIndex(components.Count, i)];
                formated.Columns.AutoFit();

            }

        }
        private void NetListcreateWorkSheet(ref Excel.Worksheet sheet, List<NetList> Nets)
        {

            sheet.Name = "NetListen Übersicht";

            for (int i = 0; i < Nets.Count; i++)
            {
                Excel.Range cell = (Excel.Range) sheet.Cells[2, i + 1];
                Excel.Borders boarder = cell.Borders;
                boarder.LineStyle = Excel.XlLineStyle.xlContinuous;
                boarder.Weight = 2.00;

                sheet.Cells[2, i + 1] = Nets[i].NetListName;
            }

            int column = 0;
            foreach (NetList listelement in Nets)
            {
                List<string> teil = listelement.Teilnehmer;
                
                int row = 3;

                foreach (string list in teil)
                {
                    sheet.Cells[row, column + 1 ] = list;
                    row++;
                }
                Excel.Range cell = (Excel.Range) sheet.Cells[2, column + 1];
                cell.Columns.AutoFit();


                column++;
            }


            for (int i = 0; i < Nets.Count; i++)
            {
                Excel.Range formated = sheet.Range[InttoExcelIndex(2,i+1), InttoExcelIndex(Nets[i].Teilnehmer.Count, i+1)];
                formated.Columns.AutoFit();

            }
        }

        private void CreateOutLineWorkSheet(ref Excel.Worksheet sheet, string sheetName, List<BaseOutlineObject> comps)
        {
            sheet.Name = sheetName;
            sheet.Cells[1, 1] = "Object type";
            sheet.Cells[1, 2] = "X_Center";
            sheet.Cells[1, 3] = "Y_Center";
            sheet.Cells[1, 4] = "X_StartPoint";
            sheet.Cells[1, 5] = "Y_StartPoint";
            sheet.Cells[1, 6] = "X_EndPoint";
            sheet.Cells[1, 7] = "Y_EndPoint";
            sheet.Cells[1, 8] = "Diameter";
            sheet.Cells[1, 9] = "Clockwise";
            for (int i = 0; i < comps.Count; i++)
            {
                if(comps[i].ToString() == "PCBInvestAPI.DataModel.OutLineObjects.ArcOutlineObject")
                {
                    var arc = (ArcOutlineObject)comps[i];
                    sheet.Cells[i + 2, 1] = "Arc";
                    sheet.Cells[i + 2, 2] = arc.X_Center;
                    sheet.Cells[i + 2, 3] = arc.Y_Center;
                    sheet.Cells[i + 2, 4] = arc.X_StartPoint;
                    sheet.Cells[i + 2, 5] = arc.Y_StartPoint;
                    sheet.Cells[i + 2, 6] = arc.X_EndPoint;
                    sheet.Cells[i + 2, 7] = arc.Y_EndPoint;
                    sheet.Cells[i + 2, 8] = arc.Diameter;
                    sheet.Cells[i + 2, 9] = arc.Clockwise;

                }
                if(comps[i].ToString() == "PCBInvestAPI.DataModel.OutLineObjects.LineOutlineObject")
                {
                    var line = (LineOutlineObject)comps[i];
                    sheet.Cells[i + 2, 1] = "Line";
                    sheet.Cells[i + 2, 2] = line.X_StartPoint;
                    sheet.Cells[i + 2, 3] = line.Y_StartPoint;
                    sheet.Cells[i + 2, 4] = line.X_EndPoint;
                    sheet.Cells[i + 2, 5] = line.Y_EndPoint;

                }

            }

        }


        private string FileNameGenerator(string Path, string FileName, string FileNameAnhang, string endName, string format)
        {
            return Path + "\\" + FileName + "_" + FileNameAnhang + endName + format;
        }

        private string InttoExcelIndex(int row, int column)
        {
               
           return  GetExcelColumnName(column) + row.ToString();   
                
        }


        private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;

            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }

            return columnName;
        }



        private string InttoColum(uint column)
        {   
            if(column > 23)
            {
                return "fault";
            }

            switch (column)
            {
                case 1:
                    return "A";
                case 2:
                    return "B";
                case 3:
                    return "C";
                case 4:
                    return "D";
                case 5:
                    return "E";
                case 6:
                    return "F";
                case 7:
                    return "G";
                case 8:
                    return "H";
                case 9:
                    return "I";
                case 10:
                    return "J";
                case 11:
                    return "K";
                case 12:
                    return "L";
                case 13:
                    return "M";
                case 14:
                    return "N";
                case 15:
                    return "O";
                case 16:
                    return "P";
                case 17:
                    return "Q";
                case 18:
                    return "R";
                case 19:
                    return "S";
                case 20:
                    return "T";
                case 21:
                    return "U";
                case 22:
                    return "V";
                case 23:
                    return "W";
                case 24:
                    return "X";
                case 25:
                    return "Y";
                case 26:
                    return "Z";
                default:
                    return "A";
            }
        }


    }


}
