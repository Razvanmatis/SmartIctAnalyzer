using ProMik.SmartIct.Interfaces.PcbInvestigator.Enums;

namespace ProMik.SmartIct.Interfaces.PcbInvestigator
{
    public interface IFunctionalAttributes
    {
        PCBObjectType ComponentType { get; }

        string Ref { get; }

        string PartName { get; }

        string LayerName { get; }

        string PackageName { get; }

        string NormalizedName { get; }

        bool IsTestPoint { get; }

        string Value { get; set; }

        int StepNo { get; }
    }
}
