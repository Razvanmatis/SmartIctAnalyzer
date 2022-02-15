using System.ComponentModel;
using System.Threading;
using System.Windows;
using Ui.Modules.ModuleName.Implementations;
using Ui.Modules.ModuleName.ViewModels;

namespace Ui.Modules.ModuleName.Views
{
    /// <summary>
    /// Interaction logic for ChartsView.xaml
    /// </summary>
    public partial class ChartsForExportView : Window
    {
        private int index;

        public ChartsForExportView()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }

        private void PieSeries_Loaded(object sender, RoutedEventArgs e)
        {
            if (++index >= 4)
            {
                ((ChartsForExportViewModel)DataContext).InitValuesForRepresentation();
                new Thread(new ThreadStart(Act)).Start();
            }
        }

        private void Act()
        {
            Thread.Sleep(PdfReportHandler.SLEEPTIMEBEFOREEXPORTIMAGE);
            Dispatcher.Invoke(() =>
            {
                ((ChartsForExportViewModel)DataContext).ExportImage();
            });
        }
    }
}
