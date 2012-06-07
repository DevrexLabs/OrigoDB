using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
            this.Startup += new StartupEventHandler(App_Startup);
        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            this.ShutdownMode = ShutdownMode.OnExplicitShutdown;
            var connectionViewModel = new ConnectionSettingsViewModel();
            var connectDialog = new ConnectWindow(connectionViewModel);
            connectDialog.ShowDialog();
            var transactionHandler = connectionViewModel.GetTransactionHandler();
            var viewModel = new MainWindowViewModel(transactionHandler);
            var window = new MainWindow();
            window.DataContext = viewModel;
            this.MainWindow = window;
            window.Show();
        }
    }
}
