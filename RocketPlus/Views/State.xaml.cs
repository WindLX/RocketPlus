using RocketPlus.ViewModels;
using System.Windows.Controls;

namespace RocketPlus.Views
{
    /// <summary>
    /// State.xaml 的交互逻辑
    /// </summary>
    public partial class State : UserControl
    {
        public State()
        {
            InitializeComponent();
            DataContext = new StateViewModel();
        }
    }
}
