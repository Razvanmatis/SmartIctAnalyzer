using Interfaces.Gui;
using Interfaces.PcbInvestigator;
using Newtonsoft.Json;
using PinInformationExtractor.Helper;
using PinInformationExtractor.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PinInformationExtractor.Implementations
{
    public class PinInformationExtractor : IPinInformationExtractor
    {
        public const string DEFJTAGPINIDENTIFIER = "tdi;tdo;tck;tms";

        private readonly ILogger logger;

        public PinInformationExtractor(ILogger logger)
        {
            this.logger = logger;
        }

        public PinInformationExtractor()
        {
        }

        public List<JtagConnectionInfo> GetAllPinInformation(IList<IPCBComponent> ics, IList<IPCBComponent> pullUps, IList<IPCBComponent> pullDowns, string jtagPinIdentifier = DEFJTAGPINIDENTIFIER)
        {
            if (string.IsNullOrEmpty(jtagPinIdentifier))
            {
                jtagPinIdentifier = DEFJTAGPINIDENTIFIER;
                logger.LogMessage("No JTAG pin identifiers were defined! Taking the default value: " + DEFJTAGPINIDENTIFIER, LogCategory.WARNING);
            }

            if (ics == null || ics.Count == 0)
            {
                logger.LogMessage("No ICs defined for getting PIN information from!", LogCategory.WARNING);
                return null;
            }

            List<JtagConnectionInfo> results = new List<JtagConnectionInfo>();
            List<IPCBComponent> jtags = GetJtags(ics, jtagPinIdentifier);
            List<IPCBComponent> plainIcs = GetIcs(jtags, ics);
            if (jtags == null || jtags.Count == 0)
            {
                logger.LogMessage("No JTAG devices found considering the actual identifiers!", LogCategory.WARNING);
                return results;
            }

            foreach (var jtag in jtags)
            {
                int amountUnknown = 0;
                int amountPullUp = 0;
                int amountPullDown = 0;
                int amountNotConnected = 0;
                int amountToIc = 0;
                int amountToJtag = 0;
                int amountJtag = 0;
                List<IPCBComponent> otherJtags = GetAllOtherJtags(jtag, jtags);
                List<PinConnectionInfo> pins = new List<PinConnectionInfo>();
                foreach (var pin in jtag.Connections)
                {
                    List<string> netNames = new List<string>();
                    foreach (var net in pin.Nets)
                    {
                        if (!netNames.Contains(net.NetName))
                        {
                            netNames.Add(net.NetName);
                        }
                    }
                    List<PinConnectionType> pinConnectionTypes = new List<PinConnectionType>();
                    if (pin.Nets.Count == 0)
                    {
                        amountNotConnected++;
                        pinConnectionTypes.Add(PinConnectionType.NOTCONNECTED);
                        pins.Add(new PinConnectionInfo(pin.PinNumber, pinConnectionTypes, netNames));
                        continue;
                    }

                    bool foundJtagPin = false;
                    IList<INetComponent> netsToCheck = new List<INetComponent>();
                    foreach (var net in pin.Nets)
                    {
                        if (!foundJtagPin && IsJtagIdentifier(net, jtagPinIdentifier))
                        {
                            foundJtagPin = true;
                        }
                        if (!netsToCheck.Contains(net))
                        {
                            netsToCheck.Add(net);
                        }
                    }

                    List<ConnectedJtagDevice> jtagDevice = null;
                    if (foundJtagPin)
                    {
                        amountJtag++;
                        pinConnectionTypes.Add(PinConnectionType.JTAG);
                    }

                    if (IncludesTheObject(netsToCheck, pullDowns))
                    {
                        amountPullDown++;
                        pinConnectionTypes.Add(PinConnectionType.PULLDOWN);
                    }

                    if (IncludesTheObject(netsToCheck, plainIcs))
                    {
                        amountToIc++;
                        pinConnectionTypes.Add(PinConnectionType.TOIC);
                    }

                    if (IncludesTheObject(netsToCheck, pullUps))
                    {
                        amountPullUp++;
                        pinConnectionTypes.Add(PinConnectionType.PULLUP);
                    }

                    if (IncludesTheObject(netsToCheck, otherJtags))
                    {
                        amountToJtag++;
                        jtagDevice = GetConnectedJtagDevices(netsToCheck, otherJtags);
                        pinConnectionTypes.Add(PinConnectionType.TOJTAG);
                    }

                    if (pinConnectionTypes.Count == 0)
                    {
                        amountUnknown++;
                        pinConnectionTypes.Add(PinConnectionType.UNKNOWN);
                    }

                    pins.Add(new PinConnectionInfo(pin.PinNumber, pinConnectionTypes, netNames, jtagDevice));
                }

                List<string> totalAmountOfNetsOfComp = new List<string>();
                foreach (var pin in jtag.Connections)
                {
                    foreach (var net in pin.Nets)
                    {
                        if (!totalAmountOfNetsOfComp.Contains(net.NetName))
                        {
                            totalAmountOfNetsOfComp.Add(net.NetName);
                        }
                    }
                }

                results.Add(new JtagConnectionInfo(
                    jtag.FunctionalAttributes.Ref + ": " + jtag.FunctionalAttributes.Value, 
                    pins, 
                    totalAmountOfNetsOfComp.Count,
                    amountUnknown,
                    amountPullUp,
                    amountPullDown,
                    amountNotConnected,
                    amountToIc,
                    amountToJtag,
                    amountJtag));
            }

            return results;
        }

        public List<JtagConnectionInfo> CreatePinInformationFile(string fileName, IList<IPCBComponent> ics, IList<IPCBComponent> pullUps, IList<IPCBComponent> pullDowns,
            string jtagPinIdentifier = DEFJTAGPINIDENTIFIER)
        {
            List<JtagConnectionInfo> results = GetAllPinInformation(ics, pullUps, pullDowns, jtagPinIdentifier);
            if (results == null || results.Count == 0)
            {
                return null;
            }

            if (File.Exists(fileName))
            {
                try
                {
                    File.Delete(fileName);
                }
                catch (Exception e)
                {
                    logger?.LogMessage("Error deleting existing file for export: " + fileName + ":" + e.Message, LogCategory.ERROR);
                    return null;
                }
            }

            if (!SaveContentIntoFile(results, fileName))
            {
                logger.LogMessage("Error saving PIN information into file!", LogCategory.ERROR);
            }

            return results;
        }

        public List<string> GetAllGroundPins(List<PinConnectionInfo> pins)
        {
            return GetAllPinsOfType(pins, PinConnectionType.PULLDOWN);
        }

        public List<string> GetAllPowerPins(List<PinConnectionInfo> pins)
        {
            return GetAllPinsOfType(pins, PinConnectionType.PULLUP);
        }

        private static List<string> GetAllPinsOfType(List<PinConnectionInfo> pins, PinConnectionType type)
        {
            List<string> names = new List<string>();
            foreach (var pin in pins)
            {
                if (pin.PinConnectionType.Contains(type) && !names.Contains(pin.PinNumber))
                {
                    names.Add(pin.PinNumber);
                }
            }

            return names;
        }

        private List<IPCBComponent> GetJtags(IList<IPCBComponent> ics, string jtagPinIdentifier)
        {
            List<IPCBComponent> list = new List<IPCBComponent>();
            foreach (var comp in ics)
            {
                bool found = false;
                foreach (var pin in comp.Connections)
                {
                    foreach (var net in pin.Nets)
                    {
                        if (IsJtagIdentifier(net, jtagPinIdentifier) && !list.Contains(comp))
                        {
                            list.Add(comp);
                            found = true;
                            break;
                        }
                    }
                    if (found)
                    {
                        break;
                    }
                }
            }

            return list;
        }

        private List<ConnectedJtagDevice> GetConnectedJtagDevices(IList<INetComponent> netsToCheck, IList<IPCBComponent> otherJtags)
        {
            List<ConnectedJtagDevice> connections = new List<ConnectedJtagDevice>();
            List<IPCBComponent> compsToFind = new List<IPCBComponent>();
            foreach (var compOther in otherJtags)
            {
                if (!GotTheSameNets(compOther.Connections, netsToCheck))
                {
                    continue;
                }

                compsToFind.Add(compOther);
            }

            foreach (var comp in compsToFind)
            {
                List<string> pins = new List<string>();
                foreach (var net in netsToCheck)
                {
                    foreach (var pin in comp.Connections)
                    {
                        if (pin.Nets.Contains(net) && !pins.Contains(pin.PinNumber))
                        {
                            pins.Add(pin.PinNumber);
                        }
                    }
                }
                connections.Add(new ConnectedJtagDevice(comp.FunctionalAttributes.Ref + ": " + comp.FunctionalAttributes.Value, pins));
            }

            return connections;
        }

        private bool GotTheSameNets(IList<IPinComponent> connections, IList<INetComponent> netsToCheck)
        {
            foreach (var pin in connections)
            {
                foreach (var net in pin.Nets)
                {
                    if (netsToCheck.Contains(net))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool SaveContentIntoFile(List<JtagConnectionInfo> content, string filePathToUse)
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
                    logger?.LogMessage(e.Message, LogCategory.ERROR);
                }
            }

            if (fileOk)
            {
                try
                {
                    string jsonString = JsonConvert.SerializeObject(content, Newtonsoft.Json.Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                    File.WriteAllText(filePathToUse, GetPrefix() + jsonString);
                    logger?.LogMessage("PIN information extraction performed for " + content.Count + " JTAG devices", LogCategory.INFO);
                    return true;
                }
                catch (Exception e)
                {
                    logger?.LogMessage("Error at serializing the settings object into JSON file: " + e.Message, LogCategory.ERROR);
                }
            }

            return false;
        }

        private string GetPrefix()
        {
            return "PinConnectionType:\n" + "UNKNOWN = 0,\n" + "PULLUP = 1,\n" + "PULLDOWN = 2,\n" + "NOTCONNECTED = 3,\n" +
                "TOIC = 4,\n" + "TOJTAG = 5,\n" + "JTAG = 6\n";
        }

        private static bool IsJtagIdentifier(INetComponent net, string jtagPinIdentifier)
        {
            return jtagPinIdentifier.Split(";").ToList().FirstOrDefault(x => net.NetName.ToLower().Contains(x.ToLower())) != null;
        }

        private List<IPCBComponent> GetIcs(List<IPCBComponent> jtags, IList<IPCBComponent> ics)
        {
            List<IPCBComponent> list = new List<IPCBComponent>();
            foreach (var comp in ics)
            {
                if (!jtags.Contains(comp) && !list.Contains(comp))
                {
                    list.Add(comp);
                }
            }

            return list;
        }

        private static List<IPCBComponent> GetAllOtherJtags(IPCBComponent comp, IList<IPCBComponent> ics)
        {
            List<IPCBComponent> list = new List<IPCBComponent>();
            foreach (var ic in ics)
            {
                if (ic == comp)
                {
                    continue;
                }

                list.Add(ic);
            }

            return list;
        }

        private static bool IncludesTheObject(IList<INetComponent> nets1, IList<IPCBComponent> objects)
        {
            foreach (var netInner in nets1)
            {
                if (ObjectsAreInNet(netInner, objects))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool ObjectsAreInNet(INetComponent netInner, IList<IPCBComponent> objects)
        {
            foreach (var obj in objects)
            {
                if (netInner.Components.Contains(obj))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
