using System.ComponentModel;
using System.Windows;

namespace Ui.Modules.ModuleName.Views
{
    /// <summary>
    /// Interaction logic for SelectionWindow.xaml
    /// </summary>
    public partial class SelectionView : Window
    {
        public SelectionView()
        {
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
        }
    }
}
