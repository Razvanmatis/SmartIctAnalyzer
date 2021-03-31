using System.ComponentModel;
using System.Windows;
using ProMik.Core.Interfaces.Events;
using Ui.Modules.ModuleName.Events;
using Ui.Modules.ModuleName.ViewModels;

namespace Ui.Modules.ModuleName.Views
{
    /// <summary>
    /// Interaction logic for OpenProjectView.xaml
    /// </summary>
    public partial class SelectBomView : Window
    {
        private readonly IEventService eventService;
        private readonly bool autoMode;

        public SelectBomView(IEventService eventService, bool autoMode)
        {
            this.eventService = eventService;
            this.autoMode = autoMode;
            InitializeComponent();
            ((SelectBomViewModel)DataContext).AutoMode = autoMode;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            eventService.Publish<SelectBomFinishEvent>(new SelectBomFinishEvent(true, autoMode));
        }
    }
}
