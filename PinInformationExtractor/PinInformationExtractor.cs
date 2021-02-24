using Interfaces.Gui;
using Interfaces.PcbInvestigator;
using Newtonsoft.Json;
using PinInformationExtractor.JSON;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace PinInformationExtractor
{
    public class PinInformationExtractor : IPinInformationExtractor
    {
        private ILogger logger;

        public PinInformationExtractor(ILogger logger)
        {
            this.logger = logger;
        }

        public PinInformationExtractor()
        {
        }

        public bool CreatePinInformationFile(string fileName, IList<IPCBComponent> ics, IList<IPCBComponent> pullUps, IList<IPCBComponent> pullDowns,
            string tdiIdentifier = "tdi", string tdoIdentifier = "tdo", string tckIdentifier = "tck", string tmsIdentifier = "tms")
        {
            if (ics == null || ics.Count == 0)
            {
                return false;
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
                    return false;
                }
            }

            List<JtagConnectionInfo> results = new List<JtagConnectionInfo>();
            List<IPCBComponent> jtags = GetJtags(ics, tdiIdentifier, tdoIdentifier, tckIdentifier, tmsIdentifier);
            List<IPCBComponent> plainIcs = GetIcs(jtags, ics);

            foreach (var jtag in jtags)
            {
                IList<INetComponent> netsOtherJtags = GetAllNetsOfOtherComponents(jtag, jtags);
                IList<INetComponent> netsOtherIcs = GetAllNetsOfOtherComponents(jtag, plainIcs);
                IList<INetComponent> netsPullUps = GetAllNetsOfOtherComponents(jtag, pullUps);
                IList<INetComponent> netsPullDowns = GetAllNetsOfOtherComponents(jtag, pullDowns);
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
                        pinConnectionTypes.Add(PinConnectionType.NOTCONNECTED);
                        pins.Add(new PinConnectionInfo(pin.PinNumber, pinConnectionTypes, netNames));
                        continue;
                    }

                    bool foundJtagPin = false;
                    IList<INetComponent> netsToCheck = new List<INetComponent>();
                    foreach (var net in pin.Nets)
                    {
                        if (!foundJtagPin && IsJtagIdentifier(net, tdiIdentifier, tdoIdentifier, tckIdentifier, tmsIdentifier))
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
                        pinConnectionTypes.Add(PinConnectionType.JTAG);
                    }

                    if (GotTheSameNet(netsToCheck, netsPullDowns))
                    {
                        pinConnectionTypes.Add(PinConnectionType.PULLDOWN);
                    }

                    if (GotTheSameNet(netsToCheck, netsOtherIcs))
                    {
                        pinConnectionTypes.Add(PinConnectionType.TOIC);
                    }

                    if (GotTheSameNet(netsToCheck, netsPullUps))
                    {
                        pinConnectionTypes.Add(PinConnectionType.PULLUP);
                    }

                    if (GotTheSameNet(netsToCheck, netsOtherJtags))
                    {
                        jtagDevice = GetConnectedJtagDevices(netsToCheck, netsOtherJtags, jtags, jtag);
                        pinConnectionTypes.Add(PinConnectionType.TOJTAG);
                    }

                    if (pinConnectionTypes.Count == 0)
                    {
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

                results.Add(new JtagConnectionInfo(jtag.FunctionalAttributes.Ref, pins, totalAmountOfNetsOfComp.Count));
            }

            return SaveContentIntoFile(results, fileName);
        }

        private List<ConnectedJtagDevice> GetConnectedJtagDevices(IList<INetComponent> netsToCheck, IList<INetComponent> netsOtherJtags, List<IPCBComponent> jtags, IPCBComponent jtag)
        {
            List<ConnectedJtagDevice> connections = new List<ConnectedJtagDevice>();
            List<IPCBComponent> compsToFind = new List<IPCBComponent>();
            foreach (var net in netsToCheck)
            {
                if (!netsOtherJtags.Contains(net))
                {
                    continue;
                }

                foreach (var comp in net.Components)
                {
                    if (comp != jtag && jtags.Contains(comp) && !compsToFind.Contains(comp))
                    {
                        compsToFind.Add(comp);
                    }
                }
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
                connections.Add(new ConnectedJtagDevice(comp.FunctionalAttributes.Ref, pins));
            }

            return connections;
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

        private static bool IsJtagIdentifier(INetComponent net, string tdiIdentifier, string tdoIdentifier, string tckIdentifier, string tmsIdentifier)
        {
            return net.NetName.ToLower().Contains(tdiIdentifier) || net.NetName.ToLower().Contains(tdoIdentifier)
                || net.NetName.ToLower().Contains(tckIdentifier) || net.NetName.ToLower().Contains(tmsIdentifier);
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

        private List<IPCBComponent> GetJtags(IList<IPCBComponent> ics, string tdiIdentifier, string tdoIdentifier, string tckIdentifier, string tmsIdentifier)
        {
            List<IPCBComponent> list = new List<IPCBComponent>();
            foreach (var comp in ics)
            {
                bool found = false;
                foreach (var pin in comp.Connections)
                {
                    foreach (var net in pin.Nets)
                    {
                        if (IsJtagIdentifier(net, tdiIdentifier, tdoIdentifier, tckIdentifier, tmsIdentifier) && !list.Contains(comp))
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

        private static IList<INetComponent> GetAllNetsOfOtherComponents(IPCBComponent comp, IList<IPCBComponent> ics)
        {
            IList<INetComponent> list = new List<INetComponent>();
            foreach (var ic in ics)
            {
                if (ic == comp)
                {
                    continue;
                }

                foreach (var pin in ic.Connections)
                {
                    foreach (var net in pin.Nets)
                    {
                        if (!list.Contains(net))
                        {
                            list.Add(net);
                        }
                    }
                }
            }

            return list;
        }

        private static bool GotTheSameNet(IList<INetComponent> nets1, IList<INetComponent> nets2)
        {
            foreach (var netInner in nets1)
            {
                foreach (var netOuter in nets2)
                {
                    if (netInner == netOuter)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
