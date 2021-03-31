using System.Collections.Generic;

namespace Ui.Modules.ModuleName.Helper
{
    public class AttributeList
    {
        public AttributeList(
            List<string> rDef,
            List<string> cDef,
            List<string> iDef,
            List<string> tDef,
            List<string> icDef,
            List<string> conDef)
        {
            RDef = rDef;
            CDef = cDef;
            IDef = iDef;
            TDef = tDef;
            IcDef = icDef;
            ConDef = conDef;
        }

        public List<string> RDef { get; private set; }

        public List<string> CDef { get; private set; }

        public List<string> IDef { get; private set; }

        public List<string> TDef { get; private set; }

        public List<string> IcDef { get; private set; }

        public List<string> ConDef { get; private set; }
    }
}
