using System.Windows.Media;
using Interfaces.PcbInvestigator;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ViewModelIc : ViewModelPCBComponentBase
    {
        private readonly ISettingsData settingsVm;

        public ViewModelIc(IPCBComponent component, ISettingsData settingsVm, IComponentHandler componentVm)
            : base(component, settingsVm, componentVm)
        {
            this.settingsVm = settingsVm;
            InitSettings();
        }

        public override void InitSettings()
        {
            SolidColorBrush brush = new SolidColorBrush
            {
                Color = settingsVm.ICColor,
            };
            BackgroundColor = brush;
            if (!settingsVm.ICNamesChecked)
            {
                Name = "IC";
            }
            else
            {
                Name = BaseComponent.FunctionalAttributes.Ref;
            }
        }
    }
}
