using RocketPlus.ViewModels;
using System.Windows.Controls;

namespace RocketPlus.Views
{
    /// <summary>
    /// Message.xaml 的交互逻辑
    /// </summary>
    public partial class Message : UserControl
    {
        public Message()
        {
            InitializeComponent();
            DataContext = new MessageViewModel();
        }
    }
}
