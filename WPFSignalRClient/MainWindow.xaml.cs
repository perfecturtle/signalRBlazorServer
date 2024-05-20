using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;

namespace WPFSignalRClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HubConnection connection;
        public MainWindow()
        {
            InitializeComponent();

            connection = new HubConnectionBuilder().WithUrl("https://localhost:7189/chathub").WithAutomaticReconnect().Build();

            connection.Reconnecting+=(sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Attempting to reconnect...";
                    messages.Items.Add(newMessage);
                });
                return Task.CompletedTask;
            };

            connection.Reconnected += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Reconnected to the server";
                    messages.Items.Clear();
                    messages.Items.Add(newMessage);
                });
                return Task.CompletedTask;
            };
            connection.Closed += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "ConnectionClosed";
                    messages.Items.Add(newMessage);
                    openConnection.IsEnabled= true;
                    sendMessage.IsEnabled= false;
                });
                return Task.CompletedTask;
            };
        }

        private async void openConnection_Click(object sender, RoutedEventArgs e)
        {
            connection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{user}:{message}";
                    messages.Items.Add(newMessage);
                });
            });

            try
            {
                await connection.StartAsync();
                messages.Items.Add("Connect Started");
                openConnection.IsEnabled = false;
                sendMessage.IsEnabled = true;
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
                throw;
            }
        }

        private async void sendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await connection.InvokeAsync("SendMessage", "WPF Client", messageInput.Text);
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
                throw;
            }
        }
    }
}