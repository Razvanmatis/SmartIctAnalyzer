using System.Windows.Media;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ViewModelInduction : ViewModelPCBComponentBase
    {
        private readonly ISettingsData settingsVm;

        public ViewModelInduction(IPCBComponent component, ISettingsData settingsVm, IComponentHandler componentVm)
            : base(component, settingsVm, componentVm)
        {
            this.settingsVm = settingsVm;
            InitSettings();
        }

        public override void InitSettings()
        {
            SolidColorBrush brush = new SolidColorBrush
            {
                Color = settingsVm.InductionColor,
            };
            BackgroundColor = brush;
            if (!settingsVm.InductionNamesChecked)
            {
                Name = "I";
            }
            else
            {
                Name = BaseComponent.FunctionalAttributes.Ref;
            }
        }
    }
}
