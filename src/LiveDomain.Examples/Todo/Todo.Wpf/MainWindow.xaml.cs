using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using LiveDomain.Core;
using LiveDomain.Enterprise.Networking.Client;
using Todo.Core;

namespace Todo.Wpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var engine = new LiveDomainClient<TodoModel>("localhost",9292);
            //var engine = Engine.LoadOrCreate<TodoModel>();
            this.DataContext = new MainWindowViewModel(engine);
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
           var control = (HeaderedContentControl) sender;
           SetVisibility(control, false);
        }

        private void SetVisibility(HeaderedContentControl control, bool visible)
        {
            (control.Header as UIElement).Visibility = visible ? Visibility.Visible : Visibility.Hidden;
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            var control = (HeaderedContentControl)sender;
            SetVisibility(control, true);
        }
    }
}
