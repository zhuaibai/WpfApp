using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfApp1.Command;
using WpfApp1.ViewModels;
using WpfApp1.Views;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ChangeLanguageToChinese(Owner,new RoutedEventArgs());

            this.DataContext = new MainWindowVM();
        }

        /// <summary>
        /// 下拉菜单
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            // 显示Popup
            myPopup.IsOpen = !myPopup.IsOpen;
        }

        /// <summary>
        /// 串口弹出界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialSettingButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SerialPortSettingWindow() { Owner = this};

            settingsWindow.ShowDialog();
        }

        /// <summary>
        /// 指令设置界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OpenSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            var settingsWindow = new SendingCommandSettingsWindow() { Owner = this};
            settingsWindow.ShowDialog();
        }

        /// <summary>
        /// 弹出日志界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Log_Click(object sender, RoutedEventArgs e)
        {
            if(Log.Visibility == Visibility)
            {
                Log_Close_Click(sender, e);
            }
            else
            {
                Log.Visibility = Visibility.Visible;

                //实现渐变动画
                //位移
                ThicknessAnimation thicknessAnimation = new ThicknessAnimation(new Thickness(0, 50, 0, -50), new Thickness(0, 0, 0, 0), new TimeSpan(0, 0, 0, 0, 200));

                //透明度
                DoubleAnimation doubleAnimation = new DoubleAnimation(0, 1, new TimeSpan(0, 0, 0, 0, 200));

                Storyboard.SetTarget(thicknessAnimation, logContent);
                Storyboard.SetTarget(doubleAnimation, logContent);
                Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath("Margin"));
                Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));

                Storyboard storyboard = new Storyboard();
                storyboard.Children.Add(thicknessAnimation);
                storyboard.Children.Add(doubleAnimation);

                storyboard.Begin();
            }
        }


        /// <summary>
        /// 关闭日志界面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Log_Close_Click(object sender, RoutedEventArgs e)
        {
            // 位移
            ThicknessAnimation thicknessAnimation = new ThicknessAnimation(
                new Thickness(0, 0, 0, 0), new Thickness(0, 50, 0, -50),
                new TimeSpan(0, 0, 0, 0, 200));
            // 透明度
            DoubleAnimation doubleAnimation = new DoubleAnimation(1, 0, new TimeSpan(0, 0, 0, 0, 200));

            Storyboard.SetTarget(thicknessAnimation, logContent);
            Storyboard.SetTarget(doubleAnimation, logContent);
            Storyboard.SetTargetProperty(thicknessAnimation, new PropertyPath("Margin"));
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("Opacity"));

            Storyboard storyboard = new Storyboard();
            storyboard.Children.Add(thicknessAnimation);
            storyboard.Children.Add(doubleAnimation);

            //动画效果完了才关闭
            storyboard.Completed += (se, ev) =>
            {
                Log.Visibility = Visibility.Collapsed;
            };
            storyboard.Begin();
        }

        /// <summary>
        /// 语言切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeLanguage(object sender, RoutedEventArgs e)
        {
            App.UpdateLanguage("Lan-en-us");
            EnglishBtn.Visibility = Visibility.Collapsed;
            ChineseBtn.Visibility = Visibility.Visible;
            
        }

        /// <summary>
        /// 语言切换
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeLanguageToChinese(object sender, RoutedEventArgs e)
        {
            ChineseBtn.Visibility = Visibility.Collapsed;
            EnglishBtn.Visibility = Visibility.Visible;
            App.UpdateLanguage("Lan-zh-cn");
        }
    }
}