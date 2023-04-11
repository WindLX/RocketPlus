using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace RocketPlus.ViewModels
{
    public partial class NavigationViewModel : ObservableObject
    {
        [ObservableProperty]
        private ObservableObject? currentViewModel;

        public NavigationViewModel()
        {
            OpenStatePage();
        }

        [RelayCommand]
        private void OpenStatePage()
        {
            CurrentViewModel = new StateViewModel();
        }

        [RelayCommand]
        private void OpenMapPage()
        {
            CurrentViewModel = new MapViewModel();
        }

        [RelayCommand]
        private void OpenDataPage()
        {
            CurrentViewModel = new DataViewModel();
        }
    }
}
