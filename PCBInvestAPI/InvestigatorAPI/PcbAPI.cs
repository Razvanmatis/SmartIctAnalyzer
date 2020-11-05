using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using PCBI;
using PCBI.Automation;
using PCBI.MathUtils;
using PCBI.Automation.DrawingParameters;
using System.Collections;
using PCBInvestAPI.DataModel;
using PCBInvestAPI.DataModel.OutLineObjects;

namespace PCBInvestAPI
{

    public class PcbAPI
    {
        private IPCBIWindow pcbWin;
        private List<IStep> _listofSteps;
        private IPCBIWindow.LoadInformation _pcbInfos;
        private string _odbPath;

        public int getInFoFromPCBInvestigator =>  (int)_pcbInfos;

        private FileInformations _fileInformations;

        public FileInformations ODBFileInfo
        {
            get { return _fileInformations; }
            set { _fileInformations = value; }
        }




        public PcbAPI()
        {

        }

        public PcbAPI(string pathtoODBFile, ReadInConfigurations.ApplicationSettings settings)
        {
            _odbPath = pathtoODBFile;
            PCBI.Automation.IAutomation.IAutomationInit(settings.PathtoPCBExe);
            //var isOdb = IAutomation.IsODBAllowed();
            _fileInformations = new FileInformations();

            pcbWin = IAutomation.CreateNewPCBIWindow(false);
            pcbWin.LoadData(_odbPath, out _pcbInfos);
            
 

        }

        public bool tryPCBisRunning(string PathtoExe)
        {

            try
            {
                PCBI.Automation.IAutomation.IAutomationInit(PathtoExe);
                pcbWin = IAutomation.CreateNewPCBIWindow(false);
            }
            catch (Exception)
            {

                return false;
            }


            if(pcbWin != null)
            {
                return true;
            }

            return false;

        }

        public List<string> getAllSteps()
        {
            List<string> StepsName = new List<string>();
            _listofSteps = new List<IStep>();
            _listofSteps = pcbWin.GetStepList();



            foreach (IStep step in _listofSteps)
            {
                StepsName.Add(step.Name);
                List<string> LayerNames = new List<string>();
                LayerNames = step.GetAllLayerNames();

                _fileInformations.ListofLayers = new List<LayerInformations>();
                for (int i = 0; i < LayerNames.Count; i++)
                {
                    LayerInformations layerInfo = new LayerInformations();
                    layerInfo.StepName = step.Name;
                    layerInfo.LayerName = LayerNames[i];
                    ILayer layer = step.GetLayer(LayerNames[i]);
                    List<IObject> objects = layer.GetAllLayerObjects();
                    layerInfo.ObjectCount = objects.Count;
                    _fileInformations.ListofLayers.Add(layerInfo);
                }


            }

            _fileInformations.Steplist = StepsName;
            return StepsName;
        }



        public void readouComponents(int stepnr, out List<TestPoint> foundedTestPoints, out List<Component> foundedComponents)
        {
            IStep step = _listofSteps[stepnr];
            foundedComponents = new List<Component>();
            foundedTestPoints = new List<TestPoint>();

            List<ICMPObject> cmp = new List<ICMPObject>();
            cmp = step.GetAllCMPObjects();

            /// ReadOut Components - TestPoints and Components

            if (cmp.Count > 0)
            {

                foreach (ICMPObject componente in cmp)
                {
                    PointD point = componente.CenterPoint;

                    string CompPos = "";

                    if (componente.PlacedTop)
                    {
                        CompPos = "top";
                    }
                    else
                    {
                        CompPos = "bottom";
                    }



                    if (componente.PartName.Contains("testpoint") || componente.UsedPackageName.Contains("TP") || componente.PartName.Contains("TP"))
                    {
                        string LayerName = componente.LayerName;
                        TestPoint newTest = new TestPoint(componente.Ref, componente.PartName, point.X, point.Y, CompPos);
                        foundedTestPoints.Add(newTest);
                    }
                    else
                    {
                        if (componente.CompHEIGHT > 0.00)
                        {
                            string compzusatz = "";
                            if (componente.Value != "")
                            {
                                compzusatz = "-" + componente.Value;
                            }
                            string LayerName = componente.LayerName;
                            Component newComp = new Component(componente.Ref, componente.PartName + compzusatz, point.X, point.Y, componente.CompHEIGHT, CompPos);
                            newComp.ComponentenOutline = getOutlinefromComponent(componente.Ref);

                            foundedComponents.Add(newComp);
                        }
                    }

                }
            }
        }
        public void readoutNestInformations(int stepnr, out List<NetList> netLists, ref List<TestPoint> tpList, ref List<Component> cpList)
        {
            netLists = new List<NetList>();

            ///Liste aller Vorhanden Netze
            List<INet> net = _listofSteps[stepnr].GetNets();
            if (net != null && net.Count > 0)
            {

                for (int i = 0; i < net.Count; i++)
                {
                    ///Teilnehmer / Componenten die im Netz befinden
                    List<INetObject> list = net[i].ComponentList;


                    for (int y = 0; y < list.Count; y++)
                    {

                        ///nets -- Erzeugt ein Netz für die benötigten Listen wo ein Testpunkt gefunden wurde
                        NetList nets = new NetList();

                        ICMPObject ob = list[y].ICMP;

                        bool foundTPinNEt = false;

                        foreach (TestPoint tp in tpList)
                        {
                            if (tp.TPname == ob.Ref)
                            {
                                nets.NetListName = net[i].NetName;
                                foundTPinNEt = true;
                            }
                        }

                        if (nets != null && foundTPinNEt)
                        {
                            List<string> netsteilnehmer = new List<string>();

                            for (int z = 0; z < list.Count; z++)
                            {
                                ICMPObject ob1 = list[z].ICMP;
                                netsteilnehmer.Add(ob1.Ref);
                            }

                            nets.Teilnehmer = netsteilnehmer;
                            netLists.Add(nets);
                        }


                    }
                }
                //Enfernen von doppelten Listeneinträge Teilnehmer
                for (int i = 0; i < netLists.Count; i++)
                {
                    List<string> ohneduplicates = netLists[i].Teilnehmer.Distinct().ToList();
                    netLists[i].Teilnehmer = ohneduplicates;
                }
                //Entfernen von doppelten Netseinträge
                List<NetList> newNetList = netLists;
                for (int i = 0; i < netLists.Count; i++)
                {
                    int count = 0;
                    for (int z = 0; z < newNetList.Count; z++)
                    {

                        if (netLists[i].NetListName == newNetList[z].NetListName)
                        {
                            count = count + 1;
                            if (count > 1)
                            {
                                newNetList.RemoveAt(z);
                            }

                        }
                    }
                }


                netLists = newNetList;
            }


            ///Zuordnung der NetListen zu den Testpunkten und Componenten
            if (netLists != null && netLists.Count > 0)
            {
                foreach (NetList netitem in netLists)
                {
                    for (int i = 0; i < netitem.Teilnehmer.Count; i++)
                    {

                        for (int z = 0; z < tpList.Count; z++)
                        {
                            if (netitem.Teilnehmer[i] == tpList[z].TPname)
                            {
                               tpList[z].NetList = netitem.NetListName;
                            }
                        }

                        for (int z = 0; z < cpList.Count; z++)
                        {
                            if (netitem.Teilnehmer[i] == cpList[z].CPName)
                            {
                                cpList[z].NetList = netitem.NetListName;
                            }
                        }
                    }
                }
            }
            ///Zuordnung der NetListen zu den Testpunkten und Componenten
        }

        public List<BaseOutlineObject> getPCBOutline(int stepnr)
        {
            IStep step = _listofSteps[stepnr];
            List<BaseOutlineObject> outline = new List<BaseOutlineObject>();

            ///PCB Outline auslesen

            IODBObject outlineObjects = step.GetPCBOutlineAsODBObject();

            IObjectSpecificsD objectSpecificsD = outlineObjects.GetSpecificsD();

            string s = objectSpecificsD.ToString();

            ISurfaceSpecificsD poly = (ISurfaceSpecificsD)objectSpecificsD;
            List<IODBObject> listX = poly.GetOutline();
            List<IPolygonSpecificsD> polyy = poly.GetPolygonOutlineD();
            List<IObjectSpecificsD> objects = polyy[0].GetOutline();

            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i].ToString() == "PCBI.Automation.IArcSpecificsD")
                {
                    IArcSpecificsD arc = (IArcSpecificsD)objects[i];
                    outline.Add(new ArcOutlineObject(arc.Center, arc.Start, arc.End, arc.ClockWise));


                }
                if (objects[i].ToString() == "PCBI.Automation.ILineSpecificsD")
                {
                    ILineSpecificsD line = (ILineSpecificsD)objects[i];
                    outline.Add(new LineOutlineObject(line.Start, line.End));
                }

            }


            ///PCB Outline auslesen

            return outline;
        }

        public List<BaseOutlineObject> getOutlinefromComponent(string compnamne)
        {
            List<BaseOutlineObject> outline = new List<BaseOutlineObject>();

            for (int i = 0; i < _listofSteps.Count; i++)
            {
                List<ICMPObject> listofAllCMPs = _listofSteps[i].GetAllCMPObjects();

                foreach (ICMPObject cmpitem in listofAllCMPs)
                {
                    if (cmpitem.Ref == compnamne)
                    {

                        List<IObjectSpecificsD> objects = cmpitem.GetOutlineD();

                        for (int u  = 0; u < objects.Count; u++)
                        {
                            if (objects[u].ToString() == "PCBI.Automation.IArcSpecificsD")
                            {
                                IArcSpecificsD arc = (IArcSpecificsD)objects[u];
                                outline.Add(new ArcOutlineObject(arc.Center, arc.Start, arc.End, arc.ClockWise));


                            }
                            if (objects[u].ToString() == "PCBI.Automation.ILineSpecificsD")
                            {
                                ILineSpecificsD line = (ILineSpecificsD)objects[u];
                                outline.Add(new LineOutlineObject(line.Start, line.End));
                            }

                        }
                    }
                }

                return outline;
            }


            return outline;
        }


        public List<string> getAllNamesfromComponents()
        {
            List<string> returnList = new List<string>();

            for (int i = 0; i < _listofSteps.Count; i++)
            {
                List<ICMPObject> listofAllCMPs = _listofSteps[i].GetAllCMPObjects();

                foreach (ICMPObject cpitem in listofAllCMPs)
                {
                    returnList.Add(cpitem.Ref);
                }
            }
            return returnList;
        }



        public void closePCB()
        {
            PCBI.Automation.IAutomation.ClosePCBIWindow(pcbWin);
            pcbWin = null;
        }


        private void getOutline(IStep actStep)
        {
            IPolyClass poly = actStep.GetPCBOutlinePoly();

            List<IObjectSpecificsD> listofElements = poly.GetOutline();


            List<string> layerNames = actStep.GetAllLayerNames();

            for (int i = 0; i < layerNames.Count; i++)
            {
                if (layerNames[i] == "ROUT")
                {
                    ILayer routLayer = actStep.GetLayer("ROUT");

                    
                    

                    routLayer.SaveLayerAsDXF(@"C:\Users\albr\Desktop\testnewUni\tgz\test.dxf");
                }
            }
 
            
            //string layername = layer.GetLayerName();


            //List<IObject> list = layer.GetAllLayerObjects();

            //for (int i = 0; i < list.Count; i++)
            //{
            //    List<IObjectSpecificsD> obj = list[i].GetOutlineD();
            //}
               
            

           //bool v =layer.SaveLayerAsDXF(@"C:\Users\albr\Desktop\testnewUni\tgz\test.dxf");







        }



    }
}
