using System;
using System.Collections.Generic;
using System.Text;

namespace PCBInvestAPI.Information
{
    public class Infos
    {
        private Info _info;
        private InfoType _infotype;
        private string _pathtoOutput;
        private string _infoMessage;


        public Infos(Info info, InfoType type, string InfoMessage="", string pathToExcelOutput="")
        {
            _info = info;
            _infotype = type;
            _pathtoOutput = pathToExcelOutput;
            _infoMessage = InfoMessage;
        }


        public void writeInfotoConsole()
        {
            string Info = "";

            string InfoType ="";

            switch (_info)
            {
                case PCBInvestAPI.Information.Info.error:
                    Info = "Error";
                    break;
                case PCBInvestAPI.Information.Info.notFound:
                    Info = "NotFound";
                    break;
                case PCBInvestAPI.Information.Info.ok:
                    Info = "Ok";
                    break;
            }

            switch (_infotype)
            {
                case PCBInvestAPI.Information.InfoType.applicationsettings:
                    InfoType = "appsettings";
                    break;
                case PCBInvestAPI.Information.InfoType.steps:
                    InfoType = "steps";
                    break;
                case PCBInvestAPI.Information.InfoType.infos:
                    InfoType = "Infos";
                    break;
                case PCBInvestAPI.Information.InfoType.tp:
                    InfoType = "TP";
                    break;
                case PCBInvestAPI.Information.InfoType.comps:
                    InfoType = "Componenten";
                    break;
                case PCBInvestAPI.Information.InfoType.pcbOutline:
                    InfoType = "PCBOutline";
                    break;
                case PCBInvestAPI.Information.InfoType.compsOutline:
                    InfoType = "ComponentenOutline";
                    break;
                case PCBInvestAPI.Information.InfoType.nets:
                    InfoType = "Nets";
                    break;
                default:
                    break;
            }

            string consoleOutput;

            consoleOutput = Info+ "-";
            consoleOutput += InfoType + "-";
            if(_infoMessage != string.Empty)
            {
                consoleOutput += _infoMessage + "-";
            }
            if(_pathtoOutput != string.Empty)
            {
                consoleOutput += _pathtoOutput;
            }


            Console.WriteLine(consoleOutput);

        }
    }

    public enum InfoType
    {   
        applicationsettings,
        steps,
        infos,
        tp,
        comps,
        nets,
        pcbOutline,
        compsOutline
    }


    public enum Info
    {   
        error,
        notFound,
        ok
    }
}
