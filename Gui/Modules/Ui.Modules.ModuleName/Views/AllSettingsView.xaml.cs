using System.ComponentModel;
using System.Windows;
using ProMik.Core.Interfaces.Events;
using Ui.Modules.ModuleName.Events;

namespace Ui.Modules.ModuleName.Views
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class AllSettingsView : Window
    {
        private readonly IEventService eventService;

        public AllSettingsView(IEventService eventService)
        {
            this.eventService = eventService;
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            eventService.Publish<CloseAllSettingsEvent>(new CloseAllSettingsEvent(true));
            e.Cancel = true;
        }
    }
}
