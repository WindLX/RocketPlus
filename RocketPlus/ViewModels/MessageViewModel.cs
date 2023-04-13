using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RocketPlus.Services;
using RocketPlus.Utils;

namespace RocketPlus.ViewModels
{
    public partial class MessageViewModel : ObservableObject
    {
        [ObservableProperty]
        private string rawMessage = "";

        [ObservableProperty]
        private bool isConnecting = false;

        [ObservableProperty]
        private bool isNotTrying = true;

        public MessageViewModel()
        {
            Client.Instace.OnRecvMessage += (msg) => RawMessage += $"> {msg.TrimStart().TrimEnd()}\n";
            Client.Instace.OnConnectStateChange += (state) =>
            {
                switch (state)
                {
                    case RocketConnectState.TryConnect:
                        IsConnecting = false;
                        IsNotTrying = false;
                        break;
                    case RocketConnectState.Connecting:
                        IsConnecting = true;
                        IsNotTrying = true;
                        break;
                    case RocketConnectState.TryDisconnect:
                        IsConnecting = true;
                        IsNotTrying = false;
                        break;
                    case RocketConnectState.Disconnect:
                        IsConnecting = false;
                        IsNotTrying = true;
                        break;
                    default:
                        break;
                }
            };
        }

        [RelayCommand]
        private async void Connect()
        {
            if (!IsConnecting)
            {
                await Client.Instace.Connect();
            }
            else
            {
                await Client.Instace.Close();
            }
        }

        [RelayCommand]
        private static async void Emergency()
        {
            await Client.Instace.Emergency();
        }

        [RelayCommand]
        private void ClearMessage()
        {
            RawMessage = string.Empty;
        }
    }
}
