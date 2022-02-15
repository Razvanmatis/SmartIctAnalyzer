using System.Windows.Media;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ViewModelCapacitor : ViewModelPCBComponentBase
    {
        private readonly ISettingsData settingsVm;

        public ViewModelCapacitor(IPCBComponent component, ISettingsData settingsVm, IComponentHandler componentVm)
            : base(component, settingsVm, componentVm)
        {
            this.settingsVm = settingsVm;
            InitSettings();
        }

        public override void InitSettings()
        {
            SolidColorBrush brush = new SolidColorBrush
            {
                Color = settingsVm.CapacitorColor,
            };
            BackgroundColor = brush;
            if (!settingsVm.CapacitorNamesChecked)
            {
                Name = "C";
            }
            else
            {
                Name = BaseComponent.FunctionalAttributes.Ref;
            }
        }
    }
}
