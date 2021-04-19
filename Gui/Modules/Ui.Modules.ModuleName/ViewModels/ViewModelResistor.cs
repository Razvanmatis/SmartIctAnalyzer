using System.Windows.Media;
using Interfaces.PcbInvestigator;
using Ui.Modules.ModuleName.Interfaces;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class ViewModelResistor : ViewModelPCBComponentBase
    {
        private readonly ISettingsData settingsVm;

        public ViewModelResistor(IPCBComponent component, ISettingsData settingsVm, IComponentHandler componentVm)
            : base(component, settingsVm, componentVm)
        {
            this.settingsVm = settingsVm;
            InitSettings();
        }

        public override void InitSettings()
        {
            SolidColorBrush brush = new SolidColorBrush
            {
                Color = settingsVm.ResistorColor,
            };
            BackgroundColor = brush;
            if (!settingsVm.ResistorNamesChecked)
            {
                Name = "R";
            }
            else
            {
                Name = BaseComponent.FunctionalAttributes.Ref;
            }
        }
    }
}
