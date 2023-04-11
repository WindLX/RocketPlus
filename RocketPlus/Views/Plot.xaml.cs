using RocketPlus.ViewModels;
using System.Windows.Controls;

namespace RocketPlus.Views
{
    /// <summary>
    /// Plot.xaml 的交互逻辑
    /// </summary>
    public partial class Plot : UserControl
    {
        public Plot()
        {
            InitializeComponent();
            DataContext = new PlotViewModel();
        }
    }
}
