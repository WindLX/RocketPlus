using CommunityToolkit.Mvvm.ComponentModel;
using RocketPlus.Models;
using RocketPlus.Services;

namespace RocketPlus.ViewModels
{
    public partial class StateViewModel : ObservableObject
    {
        [ObservableProperty]
        private MessageDataModel? rocketData;

        public StateViewModel()
        {
            Client.Instace.OnRawMessage += (msg) =>
            {
                if (MessageDataConverter.RawMessageToDataConverter(msg) != MessageDataModel.Zero)
                    RocketData = MessageDataConverter.RawMessageToDataConverter(msg);
            };
        }
    }
}
