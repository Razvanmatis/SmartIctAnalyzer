using System.ComponentModel;
using System.Windows;
using Ui.Modules.ModuleName.ViewModels;

namespace Ui.Modules.ModuleName.Views
{
    /// <summary>
    /// Interaction logic for ChartsView.xaml
    /// </summary>
    public partial class ChartsView : Window
    {
        public ChartsView()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void Chartpoint_DataClick(object sender, LiveCharts.ChartPoint chartPoint)
        {
            ChartsViewModel.HandleOnDataClick(chartPoint);
        }
    }
}
