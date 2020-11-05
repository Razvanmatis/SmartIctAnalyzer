using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Uniconverter.Gui
{
    public enum ViewType
    {
        Home,
        CompareView,
        ProjectManagment,

    }

    public interface INavigator
    {
        BaseViewModel CurrentViewModel { get; set; }
        ICommand UpdateCurrentViewModelCommand { get; }
    }
}
