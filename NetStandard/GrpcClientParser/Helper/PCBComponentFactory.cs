using System;
using System.Collections.Generic;
using System.Linq;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Implementations;
using ProMik.SmartIct.PCBComponentParser.Implementations;

namespace ProMik.SmartIct.PCBComponentParser.Helper
{
    public static class PCBComponentFactory
    {
        private static readonly IList<string> RdefList = new List<string>() { "r" };
        private static readonly IList<string> CdefList = new List<string>() { "c" };
        private static readonly IList<string> IdefList = new List<string>() { "l" };
        private static readonly IList<string> IcdefList = new List<string>() { "j" };
        private static readonly IList<string> CondefList = new List<string>() { "x" };

        public static IPCBComponent GetComponentByType(
            IGeometricAttributes geometricAttributes,
            IFunctionalAttributes functionalAttributes,
            IList<IPinComponent> connections,
            IList<string> rDef = null,
            IList<string> cDef = null,
            IList<string> iDef = null,
            IList<string> icDef = null,
            IList<string> conDef = null,
            bool useContains = false)
        {
            if (rDef == null)
            {
                rDef = RdefList;
            }

            if (cDef == null)
            {
                cDef = CdefList;
            }

            if (iDef == null)
            {
                iDef = IdefList;
            }

            if (icDef == null)
            {
                icDef = IcdefList;
            }

            if (conDef == null)
            {
                conDef = CondefList;
            }

            string beginPartName = functionalAttributes.PartName.ToLower(System.Globalization.CultureInfo.CurrentCulture);
            string beginRef = functionalAttributes.Ref.ToLower(System.Globalization.CultureInfo.CurrentCulture);
            if (MatchesType(icDef, beginPartName, beginRef, useContains))
            {
                return new PCBIc(geometricAttributes, functionalAttributes, connections);
            }
            else if (MatchesType(conDef, beginPartName, beginRef, useContains))
            {
                return new PCBConnector(geometricAttributes, functionalAttributes, connections);
            }
            else if (MatchesType(cDef, beginPartName, beginRef, useContains))
            {
                return new PCBCapacitor(geometricAttributes, functionalAttributes, connections);
            }
            else if (MatchesType(iDef, beginPartName, beginRef, useContains))
            {
                return new PCBInduction(geometricAttributes, functionalAttributes, connections);
            }
            else if (MatchesType(rDef, beginPartName, beginRef, useContains))
            {
                return new PCBResistor(geometricAttributes, functionalAttributes, connections);
            }
            else if (functionalAttributes.IsTestPoint)
            {
                return new PCBTestpoint(geometricAttributes, functionalAttributes, connections);
            }
            else
            {
                return new PCBComponent(geometricAttributes, functionalAttributes, connections);
            }
        }

        private static bool MatchesType(IList<string> identifier, string partName, string refText, bool useContains)
        {
            if (!useContains)
            {
                return identifier.FirstOrDefault(text => refText.StartsWith(
                    text, StringComparison.InvariantCultureIgnoreCase)) != null;
            }
            else
            {
                return identifier.FirstOrDefault(text => refText.Contains(
                    text, StringComparison.InvariantCultureIgnoreCase)) != null;
            }
        }
    }
}
