using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Uniconverter.Gui.Commands;

namespace Uniconverter.Gui
{
    public class Navigator: BaseViewModel , INavigator
    {
        private AppSettings Appsettings;
        private BaseViewModel _currentViewModel;
        public BaseViewModel CurrentViewModel
        {
            get
            {
                return _currentViewModel;
            }

            set
            {
                _currentViewModel = value;
                OnPropertyChanged(nameof(CurrentViewModel));
            }
        }

        public ICommand UpdateCurrentViewModelCommand { get; set; }

        public Navigator(IOptions<AppSettings> settings)
        {
            Appsettings = settings.Value;
            UpdateCurrentViewModelCommand = new UpdateCurrentViewModelCommand(this , Appsettings);
        }
    }
}
