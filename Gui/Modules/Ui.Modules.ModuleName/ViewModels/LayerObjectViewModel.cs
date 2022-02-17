using Ui.Core.Mvvm;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class LayerObjectViewModel : ViewModelBase
    {
        private string content = string.Empty;
        private bool isSelected;

        public string Content { get => content; set => SetProperty(ref content, value); }

        public bool IsSelected { get => isSelected; set => SetProperty(ref isSelected, value); }
    }
}
