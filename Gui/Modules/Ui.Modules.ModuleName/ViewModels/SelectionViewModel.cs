using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Ui.Modules.ModuleName.Views;

namespace Ui.Modules.ModuleName.ViewModels
{
    public class SelectionViewModel : BindableBase
    {
        public const string ConsiderAllProjectfile = "Consider all JTAGs within one project file";
        public const string ConsiderEachProjectfile = "Consider all JTAGs within separated project files";
        public const string ConsiderAllGeneral = "Consider all JTAGs";
        private const string LABELFIXED = "Please select JTAG devices for SVF file generation";
        private string label = string.Empty;
        private string selectedItem = string.Empty;
        private ObservableCollection<string> items = new ObservableCollection<string>();

        public SelectionViewModel()
        {
            OkCommand = new DelegateCommand(HandleHide);
            WindowCommand = new DelegateCommand<Window>(SetWindow);
        }

        public ICommand WindowCommand { get; }

        public string Label
        {
            get
            {
                return label;
            }

            set
            {
                SetProperty(ref label, value);
            }
        }

        public ICommand OkCommand
        {
            get;

            private set;
        }

        public string SelectedItem
        {
            get
            {
                return selectedItem;
            }

            set
            {
                SetProperty(ref selectedItem, value);
            }
        }

        public ObservableCollection<string> Items
        {
            get
            {
                return items;
            }

            set
            {
                SetProperty(ref items, value);
            }
        }

        public SelectionView View { get; set; }

        public void InitFields(List<string> items, string label = "")
        {
            if (string.IsNullOrEmpty(label))
            {
                label = LABELFIXED;
            }

            Label = label;
            Items.Clear();
            Items.AddRange(items);
            if (items.Count > 0)
            {
                SelectedItem = Items[0];
            }
            else
            {
                SelectedItem = string.Empty;
            }
        }

        private void HandleHide()
        {
            View?.Hide();
        }

        private void SetWindow(Window obj)
        {
            if (obj is SelectionView view)
            {
                View = view;
            }
        }
    }
}
