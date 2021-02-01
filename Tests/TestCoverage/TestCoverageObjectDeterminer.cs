using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using GrpcClientParser.Implementations;
using Interfaces.PcbInvestigator;

namespace TestCoverage
{
    public static class TestCoverageObjectDeterminer
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

        private static IList<INetComponent> GetNets(IList<INetComponent> nets, string identifier, string blacklist)
        {
            List<string> identifierList = GetListFromString(identifier);
            List<string> blacklistList = GetListFromString(blacklist);
            return nets.Where(net => ContainsIdentifier(net.NetName, identifierList) &&
                    !IsInBlackList(net.NetName, blacklistList)).ToList();
        }

        private static bool IsInBlackList(string netName, List<string> blacklistList)
        {
            return blacklistList.Any(x => x.ToLower(CultureInfo.CurrentCulture).Equals(netName.ToLower(CultureInfo.CurrentCulture)));
        }

        private static bool ContainsIdentifier(string netName, List<string> identifierList)
        {
            return identifierList.Any(x => netName.ToLower(CultureInfo.CurrentCulture).Contains(x));
        }

        private static List<string> GetListFromString(string identifier)
        {
            return identifier.Split(';').ToList();
        }
    }
}
