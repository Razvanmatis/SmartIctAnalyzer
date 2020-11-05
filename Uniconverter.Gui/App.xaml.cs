using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Uniconverter.Gui;

namespace Uniconverter.Gui
{
  
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        #region Private
        private readonly IHost host;

        #endregion

        public App()
        {


            host = Host.CreateDefaultBuilder().ConfigureServices((context, services) =>
            {
                ConfigureServices(context.Configuration, services);

            }).Build();

        }

        private void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
            services.AddSingleton<MainWindow>(s=> new MainWindow());          
            services.AddScoped<INavigator, Navigator>();
            services.AddSingleton<ApplicationViewModel>();
            services.AddSingleton<TestPointsTabelViewModel>();
            services.AddScoped<WindowViewModel>(s => new WindowViewModel(s.GetRequiredService<MainWindow>(), s.GetRequiredService<INavigator>()));

            ///Test
            



        }



        protected override void OnStartup(StartupEventArgs e)
        {

            //host.Start();

            Current.MainWindow = host.Services.GetRequiredService<MainWindow>();
            Current.MainWindow.DataContext = host.Services.GetRequiredService<WindowViewModel>();

            Current.MainWindow.Show();

            base.OnStartup(e);
        }
    }
}
