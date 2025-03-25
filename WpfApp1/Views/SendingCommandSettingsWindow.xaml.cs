using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1.ViewModels;

namespace WpfApp1.Views
{
    /// <summary>
    /// SendingCommandSettingsWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SendingCommandSettingsWindow : Window
    {
        public SendingCommandSettingsWindow()
        {
            InitializeComponent();
            this.DataContext = new SendingCommandSettingsViewModel();
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
