using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrpcClientParser.Implementations;
using Interfaces.PCBApiObjects;
using Interfaces.PcbInvestigator;

namespace GrpcClientParser
{
    public static class PCBComponentFactory
    {
        private static IList<string> rdefList = new List<string>() { "r" };
        private static IList<string> cdefList = new List<string>() { "c" };
        private static IList<string> idefList = new List<string>() { "l" };
        private static IList<string> icdefList = new List<string>() { "j" };
        private static IList<string> condefList = new List<string>() { "x" };

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
                rDef = rdefList;
            }

            if (cDef == null)
            {
                cDef = cdefList;
            }

            if (iDef == null)
            {
                iDef = idefList;
            }

            if (icDef == null)
            {
                icDef = icdefList;
            }

            if (conDef == null)
            {
                conDef = condefList;
            }

            string beginPartName = functionalAttributes.PartName.ToLower(System.Globalization.CultureInfo.CurrentCulture);
            string beginRef = functionalAttributes.Ref.ToLower(System.Globalization.CultureInfo.CurrentCulture);
            if (!useContains)
            {
                if (MatchesType(rDef, beginPartName, beginRef, false))
                {
                    return new PCBResistor(geometricAttributes, functionalAttributes, connections);
                }
                else if (MatchesType(cDef, beginPartName, beginRef, false))
                {
                    return new PCBCapacitor(geometricAttributes, functionalAttributes, connections);
                }
                else if (MatchesType(iDef, beginPartName, beginRef, false))
                {
                    return new PCBInduction(geometricAttributes, functionalAttributes, connections);
                }
                else if (MatchesType(icDef, beginPartName, beginRef, false))
                {
                    return new PCBIc(geometricAttributes, functionalAttributes, connections);
                }
                else if (MatchesType(conDef, beginPartName, beginRef, false))
                {
                    return new PCBConnector(geometricAttributes, functionalAttributes, connections);
                }
            }
            else
            {
                if (MatchesType(rDef, beginPartName, beginRef, true))
                {
                    return new PCBResistor(geometricAttributes, functionalAttributes, connections);
                }
                else if (MatchesType(cDef, beginPartName, beginRef, true))
                {
                    return new PCBCapacitor(geometricAttributes, functionalAttributes, connections);
                }
                else if (MatchesType(iDef, beginPartName, beginRef, true))
                {
                    return new PCBInduction(geometricAttributes, functionalAttributes, connections);
                }
                else if (MatchesType(icDef, beginPartName, beginRef, true))
                {
                    return new PCBIc(geometricAttributes, functionalAttributes, connections);
                }
                else if (MatchesType(conDef, beginPartName, beginRef, true))
                {
                    return new PCBConnector(geometricAttributes, functionalAttributes, connections);
                }
            }

            if (functionalAttributes.IsTestPoint)
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
                foreach (var text in identifier)
                {
                    if (partName.StartsWith(text, StringComparison.Ordinal) || refText.StartsWith(text, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                foreach (var text in identifier)
                {
                    if (partName.Contains(text, StringComparison.Ordinal) || refText.Contains(text, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
