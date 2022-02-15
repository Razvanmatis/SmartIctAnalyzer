using Newtonsoft.Json;
using ProMik.SmartIct.JtagPinInformationExtractor.Components;
using ProMik.SmartIct.JtagPinInformationExtractor.Helper;
using ProMik.SmartIct.Interfaces.Container;
using ProMik.SmartIct.Interfaces.Gui;
using ProMik.SmartIct.Interfaces.Helper;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.PCBComponentParser.Implementations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ProMik.SmartIct.JtagPinInformationExtractor.Interfaces;

namespace ProMik.SmartIct.JtagPinInformationExtractor.Implementations
{
    public class JJtagPinInformationCreator : IJtagPinInformationCreator
    {
        public const string DEFJTAGPINIDENTIFIER = "tdi;tdo;tck;tms";

        private readonly ILogger logger;

        public JJtagPinInformationCreator(ILogger logger)
        {
            this.logger = logger;
        }

        public List<JtagConnectionInfo> GetAllPinInformation(
            IList<IPCBComponent> jtags,
            IList<IPCBComponent> pullUps,
            IList<IPCBComponent> pullDowns,
            IList<IPCBComponent> others,
            List<string> gndNets,
            List<string> powerNets,
            List<IPCBComponent> allComponents,
            List<string> jtagPinIdentifier)
        {
            if (jtags == null || jtags.Count == 0)
            {
                logger.LogMessage("No JTAGs defined for getting PIN information from!", LogCategory.WARNING);
                return null;
            }

            List<JtagConnectionInfo> results = new List<JtagConnectionInfo>();
            if (jtags == null || jtags.Count == 0)
            {
                logger.LogMessage("No JTAG devices found considering the actual identifiers!", LogCategory.WARNING);
                return results;
            }

            List<IPCBComponent> plainIcs = GetAllPlainIcs(allComponents, jtags);
            foreach (var jtag in jtags)
            {
                int amountOthers = 0;
                int amountPullUp = 0;
                int amountPullDown = 0;
                int amountGnd = 0;
                int amountPower = 0;
                int amountNotConnected = 0;
                int amountToIc = 0;
                int amountToJtag = 0;
                int amountJtag = 0;
                List<IPCBComponent> otherJtags = GetAllOtherJtags(jtag, jtags);
                List<PinConnectionInfo> pins = new List<PinConnectionInfo>();
                foreach (var pin in jtag.Connections)
                {
                    List<string> netNames = new List<string>(pin.Nets.Select(net => net.NetName).ToHashSet());
                    List<PinConnectionTypeContainer> pinConnectionTypes = new List<PinConnectionTypeContainer>();
                    if (pin.Nets.Count == 0)
                    {
                        amountNotConnected++;
                        pinConnectionTypes.Add(new PinConnectionTypeContainer()
                        { PinConnectionType = PinConnectionType.NOTCONNECTED });
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

                    if (netsToCheck.Count == 1 && netsToCheck[0].Components.Count == 1)
                    {
                        amountNotConnected++;
                        pinConnectionTypes.Add(new PinConnectionTypeContainer()
                        { PinConnectionType = PinConnectionType.NOTCONNECTED });
                        pins.Add(new PinConnectionInfo(pin.PinNumber, pinConnectionTypes, netNames));
                        continue;
                    }

                    List<ConnectedJtagDevice> jtagDevice = null;
                    if (foundJtagPin)
                    {
                        amountJtag++;
                        pinConnectionTypes.Add(new PinConnectionTypeContainer() { PinConnectionType = PinConnectionType.JTAG });
                    }

                    List<IPCBComponent> connectedComponents = new List<IPCBComponent>();
                    if (IncludesShortNetObject(netsToCheck, gndNets))
                    {
                        amountGnd++;
                        pinConnectionTypes.Add(new PinConnectionTypeContainer()
                        { PinConnectionType = PinConnectionType.GND, ConnectedComponents = new List<IPCBComponent>() { new DirectGndComponent() } });
                    }
                    else if (IncludesShortNetObject(netsToCheck, powerNets))
                    {
                        amountPower++;
                        pinConnectionTypes.Add(new PinConnectionTypeContainer()
                        {
                            PinConnectionType =
                            PinConnectionType.POWER,
                            ConnectedComponents = new List<IPCBComponent>() { new DirectPowerComponent() }
                        });
                    }
                    else
                    {
                        if (IncludesTheObject(netsToCheck, pullUps, connectedComponents))
                        {
                            amountPullUp++;
                            pinConnectionTypes.Add(new PinConnectionTypeContainer()
                            { PinConnectionType = PinConnectionType.PULLUP, ConnectedComponents = connectedComponents });
                        }

                        connectedComponents = new List<IPCBComponent>();
                        if (IncludesTheObject(netsToCheck, pullDowns, connectedComponents))
                        {
                            amountPullDown++;
                            pinConnectionTypes.Add(new PinConnectionTypeContainer()
                            { PinConnectionType = PinConnectionType.PULLDOWN, ConnectedComponents = connectedComponents });
                        }
                    }

                    connectedComponents = new List<IPCBComponent>();
                    if (IncludesTheObject(netsToCheck, plainIcs, connectedComponents))
                    {
                        amountToIc++;
                        pinConnectionTypes.Add(new PinConnectionTypeContainer()
                        { PinConnectionType = PinConnectionType.TOIC, ConnectedComponents = connectedComponents });
                    }

                    connectedComponents = new List<IPCBComponent>();
                    if (IncludesTheObject(netsToCheck, otherJtags, connectedComponents))
                    {
                        amountToJtag++;
                        jtagDevice = GetConnectedJtagDevices(netsToCheck, otherJtags);

                        // TO DO: CHECK FOR CORRECTNESS!!
                        connectedComponents = new List<IPCBComponent>(jtags.Where(ic => jtagDevice.Any(x => x.Name.Equals(ic.FunctionalAttributes.Ref))));


                        pinConnectionTypes.Add(new PinConnectionTypeContainer()
                        { PinConnectionType = PinConnectionType.TOJTAG, ConnectedComponents = connectedComponents });
                    }

                    if (pinConnectionTypes.Count == 0)
                    {
                        connectedComponents = new List<IPCBComponent>();
                        if (IncludesTheObject(netsToCheck, others, connectedComponents))
                        {
                            amountOthers++;
                            pinConnectionTypes.Add(new PinConnectionTypeContainer()
                            { PinConnectionType = PinConnectionType.OTHER, ConnectedComponents = connectedComponents });
                        }
                        else
                        {
                            logger?.LogMessage(
                                "PIN test type not found: " + pin.PinNumber + " of JTAG device: " +
                                jtag.FunctionalAttributes.Ref + ": " + jtag.FunctionalAttributes.Value,
                                LogCategory.ERROR);
                            pinConnectionTypes.Add(new PinConnectionTypeContainer()
                            { PinConnectionType = PinConnectionType.INVALID, ConnectedComponents = connectedComponents });
                        }
                    }

                    pins.Add(new PinConnectionInfo(pin.PinNumber, pinConnectionTypes, netNames, jtagDevice));
                }

                HashSet<string> totalAmountOfNetsOfComp = new HashSet<string>();
                foreach (var pin in jtag.Connections)
                {
                    foreach (var net in pin.Nets)
                    {
                        totalAmountOfNetsOfComp.Add(net.NetName);
                    }
                }

                results.Add(new JtagConnectionInfo(
                    jtag.FunctionalAttributes.Ref
                    + (!string.IsNullOrEmpty(jtag.FunctionalAttributes.Value) ? "_" + jtag.FunctionalAttributes.Value : ""),
                    pins,
                    totalAmountOfNetsOfComp.Count,
                    amountOthers,
                    amountPullUp,
                    amountPullDown,
                    amountNotConnected,
                    amountToIc,
                    amountToJtag,
                    amountJtag,
                    amountGnd,
                    amountPower));
            }

            return results;
        }

        public List<JtagConnectionInfo> CreatePinInformationFile(
            string fileName,
            IList<IPCBComponent> jtags,
            IList<IPCBComponent> pullUps,
            IList<IPCBComponent> pullDowns,
            IList<IPCBComponent> others,
            List<string> gndNets, List<string> powerNets,
            List<IPCBComponent> allComponents,
            List<string> jtagPinIdentifier)
        {
            List<JtagConnectionInfo> results = GetAllPinInformation(
                jtags, pullUps, pullDowns, others, gndNets, powerNets, allComponents, jtagPinIdentifier);
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
                    logger?.LogMessage(
                        "Error deleting existing file for export: " + fileName + ":" + e.Message, LogCategory.ERROR);
                    return null;
                }
            }

            if (!SaveContentIntoFile(results, fileName))
            {
                logger.LogMessage("Error saving PIN information into file!", LogCategory.ERROR);
            }

            return results;
        }

        private static bool IncludesShortNetObject(IList<INetComponent> netsToCheck, List<string> netsFromSettings)
        {
            foreach (var net in netsToCheck)
            {
                if (netsFromSettings.FirstOrDefault(x => x.ToLower().Equals(net.NetName.ToLower())) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private static List<IPCBComponent> GetAllPlainIcs(IList<IPCBComponent> components, IList<IPCBComponent> jtags)
        {
            return components.Where(comp => comp is PCBIc ic && !jtags.Contains(ic)).ToList();
        }

        private List<ConnectedJtagDevice> GetConnectedJtagDevices(
            IList<INetComponent> netsToCheck, IList<IPCBComponent> otherJtags)
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
                connections.Add(new ConnectedJtagDevice(
                    comp, comp.FunctionalAttributes.Ref + ": " + comp.FunctionalAttributes.Value, pins));
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
                    string jsonString = JsonConvert.SerializeObject(content, Formatting.Indented,
                        new JsonSerializerSettings
                        {
                            NullValueHandling = NullValueHandling.Ignore
                        });
                    File.WriteAllText(filePathToUse, GetPrefix() + jsonString);
                    logger?.LogMessage(
                        "PIN information extraction performed for " + content.Count + " JTAG devices", LogCategory.INFO);
                    return true;
                }
                catch (Exception e)
                {
                    logger?.LogMessage(
                        "Error at serializing the settings object into JSON file: " + e.Message, LogCategory.ERROR);
                }
            }

            return false;
        }

        private string GetPrefix()
        {
            return "PinConnectionType:\n" + "UNKNOWN = 0,\n" + "PULLUP = 1,\n" + "PULLDOWN = 2,\n" + "NOTCONNECTED = 3,\n" +
                "TOIC = 4,\n" + "TOJTAG = 5,\n" + "JTAG = 6\n" + "GND = 7\n" + "POWER = 8\n";
        }

        private static bool IsJtagIdentifier(INetComponent net, List<string> jtagPinIdentifier)
        {
            return jtagPinIdentifier.FirstOrDefault(
                x => net.NetName.ToLower().Contains(x.ToLower())) != null;
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

        private static bool IncludesTheObject(
            IList<INetComponent> nets1, IList<IPCBComponent> objects, List<IPCBComponent> connectedComponents)
        {
            bool found = false;
            foreach (var netInner in nets1)
            {
                if (ObjectsAreInNet(netInner, objects, connectedComponents))
                {
                    found = true;
                }
            }

            return found;
        }

        private static bool ObjectsAreInNet(
            INetComponent netInner, IList<IPCBComponent> objects, List<IPCBComponent> connectedComponents)
        {
            bool found = false;
            foreach (var obj in objects)
            {
                if (netInner.Components.Contains(obj))
                {
                    if (!connectedComponents.Contains(obj))
                    {
                        connectedComponents.Add(obj);
                    }
                    found = true;
                }
            }

            return found;
        }
    }
}
