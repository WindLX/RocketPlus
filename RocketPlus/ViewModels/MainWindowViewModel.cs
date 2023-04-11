using CommunityToolkit.Mvvm.ComponentModel;

namespace RocketPlus.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private MessageViewModel? messageViewModel;

        [ObservableProperty]
        private StateViewModel? stateViewModel;

        [ObservableProperty]
        private MapViewModel? mapViewModel;

        [ObservableProperty]
        private DataViewModel? dataViewModel;

        [ObservableProperty]
        private PlotViewModel? plotViewModel;

        public MainWindowViewModel()
        {
            MessageViewModel = new MessageViewModel();
            StateViewModel = new StateViewModel();
            MapViewModel = new MapViewModel();
            DataViewModel = new DataViewModel();
            PlotViewModel = new PlotViewModel();
        }
    }
}