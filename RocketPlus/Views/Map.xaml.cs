using RocketPlus.ViewModels;
using System.Windows.Controls;

namespace RocketPlus.Views
{
    /// <summary>
    /// Map.xaml 的交互逻辑
    /// </summary>
    public partial class Map : UserControl
    {
        public Map()
        {
            InitializeComponent();
            DataContext = new MapViewModel();
        }
    }
}
