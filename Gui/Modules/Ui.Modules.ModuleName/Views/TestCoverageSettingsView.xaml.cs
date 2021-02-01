using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ProMik.Services.Interfaces;
using Ui.Modules.ModuleName.Events;

namespace Ui.Modules.ModuleName.Views
{
    /// <summary>
    /// Interaction logic for TestCoverageSettings.xaml
    /// </summary>
    public partial class TestCoverageSettingsView : Window
    {
        private IEventService eventService;

        public TestCoverageSettingsView(IEventService eventService)
        {
            this.eventService = eventService;
            InitializeComponent();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            eventService.Publish<CloseTestCoverageSettingsEvent>(new CloseTestCoverageSettingsEvent(true));
        }
    }
}
