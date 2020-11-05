using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using Uniconverter.Gui;
using Uniconverter.Core;

namespace Uniconverter.Gui.Commands
{
    public class UpdateCurrentViewModelCommand : ICommand
    {
        private AppSettings _appSettingst;
        public event EventHandler CanExecuteChanged;

        private FileHandler _CompletFileHandler;
        private ApplicationViewModel _app;

        private readonly INavigator _navigator;
        //private readonly IRootViewModelFactory _viewModelFactory;

        public UpdateCurrentViewModelCommand(INavigator navigator, AppSettings settings)
        {
            _navigator = navigator;
            _appSettingst = settings;
            _CompletFileHandler = new FileHandler(_appSettingst.PathToPCBInvestConverter);


            _app = new ApplicationViewModel(ref _CompletFileHandler);
            _navigator.CurrentViewModel = _app;

            // _viewModelFactory = viewModelFactory;
        }

        public bool CanExecute(object parameter)
        {

            if (parameter is ViewType)
            {
                ViewType viewType = (ViewType)parameter;

                switch (viewType)
                {
                    case ViewType.Home:
                        return true;
                    case ViewType.CompareView:
                        //TODO: Abfrage ob die Anzahl der eingelesen Dateien größer als Eins ist.
                        return true;
                    case ViewType.ProjectManagment:
                        return _appSettingst.ProjectManagmentActive;
                    default:
                        return true;
                }
            }

            return true;
        }

        public void Execute(object parameter)
        {
            if (parameter is ViewType)
            {
                ViewType viewType = (ViewType)parameter;

                // _navigator.CurrentViewModel = _viewModelFactory.CreateViewModel(viewType);


                switch (viewType)
                {
                    case ViewType.Home:
                        _navigator.CurrentViewModel = _app;
                        break;
                    case ViewType.CompareView:
                        _navigator.CurrentViewModel = new TestPointsTabelViewModel(ref _CompletFileHandler);
                        break;
                    case ViewType.ProjectManagment:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
