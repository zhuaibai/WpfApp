﻿using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1.UserControls
{
    /// <summary>
    /// GB6042.xaml 的交互逻辑
    /// </summary>
    public partial class GB6042 : UserControl
    {
        public GB6042()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 点击概览图
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ElementView_Click(object sender, RoutedEventArgs e)
        {
            SettingView.Visibility = Visibility.Collapsed;

        }

        /// <summary>
        /// 参数设置界面显示
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ElementSetting_Click(object sender, RoutedEventArgs e)
        {
            SettingView.Visibility = Visibility.Visible;
        }
    }
}
