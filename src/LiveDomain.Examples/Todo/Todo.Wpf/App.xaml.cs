using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Windows;
using Todo.Core;

namespace Todo.Wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            
            Startup += App_Startup;
            
            DispatcherUnhandledException += (s, e) =>
            {
                MessageBox.Show("Unhandled exception, shutting down: " +
                                e.Exception.Message);
                e.Handled = true;
                Current.Shutdown();
            };
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var connectionViewModel = new ConnectionSettingsViewModel();
            var connectDialog = new ConnectWindow(connectionViewModel);
            var result = connectDialog.ShowDialog();
            if(result ?? false)
            {
                var transactionHandler = connectionViewModel.GetTransactionHandler();
                var viewModel = new MainWindowViewModel(transactionHandler);
                var window = new MainWindow();
                window.DataContext = viewModel;
                this.MainWindow = window;
                window.Show();
            }
            else Current.Shutdown();
        }
    }
}
