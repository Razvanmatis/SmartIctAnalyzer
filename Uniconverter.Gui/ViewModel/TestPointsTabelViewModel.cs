using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using Uniconverter.Core.DataModel;
using Uniconverter.Core;


namespace Uniconverter.Gui
{
    public class TestPointsTabelViewModel : BaseViewModel, IPageViewModel
    {
        private ObservableCollection<TestPoint> _activList;
        private AppSettings _settings;

        private FileHandler file;

        public TestPointsTabelViewModel(ref FileHandler fileHandler)
        {
            file = fileHandler;

        }


        

        public ObservableCollection<TestPoint> TestPoints
        {
            get { return _activList; }
            set { _activList = value; }
        }


        private void OnLoadTableData(object path)
        {


        }

        private void OnTestfunktion(object obj)
        {
            MessageBox.Show("Von Object TestPoints : "+obj.ToString());
        }
    }




}
