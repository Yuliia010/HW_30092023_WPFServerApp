using HW_30092023_WPFServerApp.Net;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static HW_30092023_WPFServerApp.Net.NetworkConnection;

namespace HW_30092023_WPFServerApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        NetworkConnection connect;
        ObservableCollection<clientLogs> clientLogs;

        public MainWindow()
        {
            InitializeComponent();
            clientLogs = new ObservableCollection<clientLogs>();
            logListView.ItemsSource = clientLogs;
            connect = new NetworkConnection();
            CheckConnection();
        }

        private async Task CheckConnection()
        {
            await Task.Run(async () =>
            {
                while (true)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        if (connect.isStart())
                        {
                            btn_Start.Background = new SolidColorBrush(Colors.LightGreen);
                            btn_Start.Content = "Connected";
                            lb_connCount.Content = connect.GetActiveConnectionsCount();
                            clientLogs.Clear(); 
                            var logs = connect.GetLogs();
                            foreach (var log in logs)
                            {
                                clientLogs.Add(log);
                            }
                        }
                        else
                        {
                            btn_Start.Background = new SolidColorBrush(Colors.IndianRed);
                            btn_Start.Content = "Disconnected";
                        }
                    });

                    await Task.Delay(1000);
                }
            });
        }

        private async void btn_Start_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await connect.StartServer();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async void Window_Closed(object sender, EventArgs e)
        {
           await connect.StartServer();

        }
    }
}
