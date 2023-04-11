using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RocketPlus.Models;
using RocketPlus.Services;
using RocketPlus.Utils;
using System;

namespace RocketPlus.ViewModels
{
    public partial class MapViewModel : ObservableObject
    {
        [ObservableProperty]
        private Uri mapUri;

        [ObservableProperty]
        private MapDataModel mapData = new();

        public MapViewModel()
        {
            Random ra = new();
            mapData = new MapDataModel()
            {
                DateTime = DateTime.Now,
                Position = new Position
                {
                    Lat = ra.NextFloat(103.930759F, 103.931687F),
                    Lon = ra.NextFloat(30.742709F, 30.744032F),
                }
            };
            mapUri = DataToUri(mapData);
            Client.Instace.OnRawMessage += (msg) =>
            {
                if (MessageDataConverter.RawMessageToMapConverter(msg).Position != Position.Zero)
                {
                    MapUri = DataToUri(MessageDataConverter.RawMessageToMapConverter(msg));
                    MapData = MessageDataConverter.RawMessageToMapConverter(msg);
                }
            };
        }

        private static Uri DataToUri(MapDataModel mapData)
        {
            var key = "04b77c62516855fdc4db68485faf2541";
            var zoom = 16;
            var location = $"{mapData.Position.Lat},{mapData.Position.Lon}";
            var size = "1800*320";
            var markers = $"mid,0xFF0000,A:{location}";
            return new Uri($"https://restapi.amap.com/v3/staticmap?zoom={zoom}&location={location}&size={size}&markers={markers}&key={key}");
        }

        [RelayCommand]
        private static void Show()
        {
            Client.Instace.OnRawMessage += (msg) =>
            {
                if (MessageDataConverter.RawMessageToPostureConverter(msg) != Vector3.Zero)
                    UnityClient.Instace.Send(MessageDataConverter.RawMessageToPostureConverter(msg).ToString());
            };
        }
    }
}
