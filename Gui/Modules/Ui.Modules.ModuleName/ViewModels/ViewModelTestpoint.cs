using System.Windows.Media;
using ProMik.SmartIct.Interfaces.PcbInvestigator;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ViewModelTestpoint : ViewModelPCBComponentBase
    {
        private readonly ISettingsData settingsVm;

        public ViewModelTestpoint(IPCBComponent component, ISettingsData settingsVm, IComponentHandler componentVm)
            : base(component, settingsVm, componentVm)
        {
            this.settingsVm = settingsVm;
            InitSettings();
        }

        public override void InitSettings()
        {
            SolidColorBrush brush = new SolidColorBrush
            {
                Color = settingsVm.TestpointColor,
            };
            BackgroundColor = brush;
            if (!settingsVm.TestpointNamesChecked)
            {
                Name = string.Empty;
            }
            else
            {
                Name = BaseComponent.FunctionalAttributes.Ref;
            }
        }
    }
}
