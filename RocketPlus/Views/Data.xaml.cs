using RocketPlus.ViewModels;
using System.Windows.Controls;

namespace RocketPlus.Views
{
    /// <summary>
    /// Data.xaml 的交互逻辑
    /// </summary>
    public partial class Data : UserControl
    {
        public Data()
        {
            InitializeComponent();
            DataContext = new DataViewModel();
        }
    }
}
