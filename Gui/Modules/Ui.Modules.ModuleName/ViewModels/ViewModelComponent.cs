using System.Windows.Media;
using Interfaces.PcbInvestigator;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ViewModelComponent : ViewModelPCBComponentBase
    {
        private readonly ISettingsData settingsVm;

        public ViewModelComponent(IPCBComponent component, ISettingsData settingsVm, IComponentHandler componentVm)
            : base(component, settingsVm, componentVm)
        {
            this.settingsVm = settingsVm;
            InitSettings();
        }

        public override void InitSettings()
        {
            SolidColorBrush brush = new SolidColorBrush
            {
                Color = settingsVm.CompColor,
            };
            BackgroundColor = brush;
            if (!settingsVm.CompNamesChecked)
            {
                Name = "Comp";
            }
            else
            {
                Name = BaseComponent.FunctionalAttributes.Ref;
            }
        }
    }
}
