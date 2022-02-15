using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class InputBoxViewModel : BindableBase
    {
        private int height;
        private int width;
        private string text = string.Empty;
        private string title = string.Empty;
        private string info = string.Empty;

        public InputBoxViewModel()
        {
            ClickCommand = new DelegateCommand(() => View?.Close());
            WindowCommand = new DelegateCommand<Window>(SetWindow);
        }

        public ICommand WindowCommand { get; }

        public int Height
        {
            get
            {
                return height;
            }

            set
            {
                SetProperty(ref height, value);
            }
        }

        public int Width
        {
            get
            {
                return width;
            }

            set
            {
                SetProperty(ref width, value);
            }
        }

        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                SetProperty(ref text, value);
            }
        }

        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                SetProperty(ref title, value);
            }
        }

        public Window View { get; set; }

        public string Info
        {
            get
            {
                return info;
            }

            set
            {
                SetProperty(ref info, value);
            }
        }

        public ICommand ClickCommand { get; }

        private void SetWindow(Window obj)
        {
            View = obj;
        }
    }
}
