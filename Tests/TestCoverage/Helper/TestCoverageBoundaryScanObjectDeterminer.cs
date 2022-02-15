using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.PCBComponentParser.Implementations;

namespace ProMik.SmartIct.TestCoverageDeterminer.Helper
{
    public static class TestCoverageBoundaryScanObjectDeterminer
    {
        public static IList<IPCBComponent> GetPullUpDownResistors(IList<INetComponent> gndNets)
        {
            return GetResistors(gndNets);
        }

        public static IList<IPCBComponent> GetIcs(IList<INetComponent> jtagNets)
        {
            IList<IPCBComponent> list = new List<IPCBComponent>();
            foreach (var net in jtagNets)
            {
                foreach (var comp in net.Components)
                {
                    if (comp is PCBIc && !list.Contains(comp))
                    {
                        list.Add(comp);
                    }
                }
            }

            return list;
        }

        public static IList<INetComponent> GetGndNets(IList<INetComponent> nets, string gndIdentifier, string gndBlacklist)
        {
            return GetNets(nets, gndIdentifier, gndBlacklist);
        }

        public static IList<INetComponent> GetPowerNets(IList<INetComponent> nets, string powerIdentifier, string powerBlacklist)
        {
            return GetNets(nets, powerIdentifier, powerBlacklist);
        }

        public static IList<INetComponent> GetJtagNets(IList<INetComponent> nets, string jtagIdentifier, string jtagBlacklist)
        {
            return GetNets(nets, jtagIdentifier, jtagBlacklist);
        }

        public static IList<IPCBComponent> GetOthers(
            IList<IPCBComponent> ics, IList<IPCBComponent> pullDowns, IList<IPCBComponent> pullUps)
        {
            IList<IPCBComponent> resistors = new List<IPCBComponent>(pullDowns);
            foreach (var comp in pullUps)
            {
                resistors.Add(comp);
            }

            IList<IPCBComponent> compsToUse = new List<IPCBComponent>();
            foreach (var comp in ics)
            {
                foreach (var pin in comp.Connections)
                {
                    foreach (var net in pin.Nets)
                    {
                        foreach (var compToUse in net.Components)
                        {
                            if (!compsToUse.Contains(compToUse)
                                && !IsComponentInList(compToUse, ics) && !IsComponentInList(compToUse, resistors))
                            {
                                compsToUse.Add(compToUse);
                            }
                        }
                    }
                }
            }

            return compsToUse;
        }

        public static bool IsComponentInList(IPCBComponent comp, IList<IPCBComponent> list)
        {
            return list.FirstOrDefault(ic => comp == ic) != null;
        }

        private static IList<IPCBComponent> GetResistors(IList<INetComponent> nets)
        {
            IList<IPCBComponent> list = new List<IPCBComponent>();
            foreach (var net in nets)
            {
                foreach (var comp in net.Components)
                {
                    if (comp is PCBResistor && !list.Contains(comp))
                    {
                        list.Add(comp);
                    }
                }
            }

            return list;
        }

        private static IList<INetComponent> GetNets(IList<INetComponent> nets, string netIdentifier, string netBlacklist)
        {
            List<string> netIdentifierList = GetListFromString(netIdentifier);
            List<string> netBlacklistList = GetListFromString(netBlacklist);
            return nets.Where(net => ContainsIdentifier(net.NetName, netIdentifierList) &&
                !IsInBlackList(net.NetName, netBlacklistList)).ToList();
        }

        private static bool IsInBlackList(string netName, List<string> blacklistList)
        {
            return blacklistList.Any(
                x => x.ToLower(CultureInfo.CurrentCulture).Equals(netName.ToLower(CultureInfo.CurrentCulture)));
        }

        private static bool ContainsIdentifier(string netName, List<string> identifierList)
        {
            return identifierList.Any(
                x => netName.ToLower(CultureInfo.CurrentCulture).Contains(x.ToLower(CultureInfo.CurrentCulture)));
        }

        private static List<string> GetListFromString(string identifier)
        {
            return identifier.Split(';').ToList();
        }
    }
}
