using System.Windows;
using Microsoft.AspNetCore.SignalR.Client;

namespace WPFSignalRClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HubConnection hubConnection;
        HubConnection counterConnection;

        public MainWindow()
        {
            InitializeComponent();

            hubConnection = new HubConnectionBuilder().WithUrl("https://localhost:7189/chathub").WithAutomaticReconnect().Build();
            counterConnection = new HubConnectionBuilder().WithUrl("https://localhost:7189/counterhub").WithAutomaticReconnect().Build();


            hubConnection.Reconnecting+=(sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Attempting to reconnect...";
                    messages.Items.Add(newMessage);
                });
                return Task.CompletedTask;
            };

            hubConnection.Reconnected += (sender) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = "Reconnected to the server";
                    messages.Items.Clear();
                    messages.Items.Add(newMessage);
                });
                return Task.CompletedTask;
            };
            hubConnection.Closed += (sender) =>
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
            hubConnection.On<string, string>("ReceiveMessage", (user, message) =>
            {
                this.Dispatcher.Invoke(() =>
                {
                    var newMessage = $"{user}:{message}";
                    messages.Items.Add(newMessage);
                });
            });

            try
            {
                await hubConnection.StartAsync();
                messages.Items.Add("Connect Started");
                openConnection.IsEnabled = false;
                sendMessage.IsEnabled = true;
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
                
            }
        }

        private async void sendMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await hubConnection.InvokeAsync("SendMessage", "WPF Client", messageInput.Text);
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
            }
        }

        private async void openCounter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await counterConnection.StartAsync();
                openCounter.IsEnabled = false;
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
            }
        }

        private async void incrementCounter_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await counterConnection.InvokeAsync("AddToTotal", "WPF client", 1);
            }
            catch (Exception ex)
            {
                messages.Items.Add(ex.Message);
            }
        }
    }
}