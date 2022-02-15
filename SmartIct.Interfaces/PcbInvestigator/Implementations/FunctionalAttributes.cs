using System;
using ProMik.SmartIct.Interfaces.PcbInvestigator.Enums;

namespace ProMik.SmartIct.Interfaces.PcbInvestigator.Implementations
{
    public class FunctionalAttributes : IFunctionalAttributes
    {
        private readonly PCBObjectType componentType;
        private readonly string reference;
        private readonly string partName;
        private readonly int stepNo;
        private readonly string layerName;
        private readonly string normalizedName;
        private readonly string packageName;
        private readonly Func<IFunctionalAttributes, bool> isTestPoint;

        public FunctionalAttributes(
            PCBObjectType componentType,
            string reference,
            string partName,
            string layerName,
            string packageName,
            string normalizedName,
            Func<IFunctionalAttributes, bool> isTestPoint,
            int stepNo,
            string value = "")
        {
            this.componentType = componentType;
            this.reference = reference;
            this.partName = partName;
            this.packageName = packageName;
            this.layerName = layerName;
            this.normalizedName = normalizedName;
            this.isTestPoint = isTestPoint;
            Value = value;
            this.stepNo = stepNo;
        }

        public PCBObjectType ComponentType
        {
            get
            {
                return componentType;
            }
        }

        public string Ref
        {
            get
            {
                return reference;
            }
        }

        public string PartName
        {
            get
            {
                return partName;
            }
        }

        public string LayerName
        {
            get
            {
                return layerName;
            }
        }

        public string NormalizedName
        {
            get
            {
                return normalizedName;
            }
        }

        public string PackageName
        {
            get
            {
                return packageName;
            }
        }

        public bool IsTestPoint
        {
            get
            {
                return isTestPoint(this);
            }
        }

        public string Value
        {
            get;
            set;
        }

        public int StepNo
        {
            get
            {
                return stepNo;
            }
        }
    }
}
