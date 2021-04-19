using System.Windows.Media;
using Interfaces.PcbInvestigator;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ViewModelConnector : ViewModelPCBComponentBase
    {
        private readonly ISettingsData settingsVm;

        public ViewModelConnector(IPCBComponent component, ISettingsData settingsVm, IComponentHandler componentVm)
            : base(component, settingsVm, componentVm)
        {
            this.settingsVm = settingsVm;
            InitSettings();
        }

        public override void InitSettings()
        {
            SolidColorBrush brush = new SolidColorBrush
            {
                Color = settingsVm.ConnectorColor,
            };
            BackgroundColor = brush;
            if (!settingsVm.ConnectorNamesChecked)
            {
                Name = "CONN";
            }
            else
            {
                Name = BaseComponent.FunctionalAttributes.Ref;
            }
        }
    }
}
