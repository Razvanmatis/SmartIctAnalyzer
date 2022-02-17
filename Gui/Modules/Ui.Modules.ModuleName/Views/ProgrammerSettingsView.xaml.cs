using System.Windows;

namespace Ui.Modules.ModuleName.Views
{
    /// <summary>
    /// Interaction logic for ProgrammerSettingsView.xaml
    /// </summary>
    public partial class ProgrammerSettingsView : Window
    {
        public ProgrammerSettingsView()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
