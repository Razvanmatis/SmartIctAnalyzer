using System.Windows.Controls;
using Ui.Modules.ModuleName.Interfaces;
using Ui.Modules.ModuleName.ViewModels;

namespace Ui.Modules.ModuleName.Views
{
    /// <summary>
    /// Interaction logic for ViewA.xaml
    /// </summary>
    public partial class ComponentView : UserControl
    {
        public ComponentView()
        {
            InitializeComponent();
            ((ComponentViewModel)DataContext).Scrollviewer = Scrollviewer;
        }
    }
}
