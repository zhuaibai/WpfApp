using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// RingUC.xaml 的交互逻辑
    /// </summary>
    public partial class RingUC : UserControl
    {
        public RingUC()
        {
            InitializeComponent();
            SizeChanged += RingUC_SizeChanged;//随界面改变大小    
        }

        int flag = 1;
        

        private void RingUC_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Drug();
            
        }

        
        /// <summary>
        /// 百分百 比如60
        /// </summary>
        public int PercentValue
        {
            get { return (int)GetValue(PrecentValueProperty); }
            set { SetValue(PrecentValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PercentValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrecentValueProperty =
            DependencyProperty.Register("PercentValue", typeof(int), typeof(RingUC),new PropertyMetadata(0,OnPercentValueChanged));

        /// <summary>
        /// 属性回调方法
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private static void OnPercentValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ringUC = d as RingUC;
            if (ringUC != null)
            {
                // 获取新值和旧值
                int oldValue = (int)e.OldValue;
                int newValue = (int)e.NewValue;

                // 在这里添加属性值变更时要执行的操作
                // 例如，更新控件的外观、触发事件等
                System.Diagnostics.Debug.WriteLine($"PercentValue 从 {oldValue} 变更为 {newValue}");
                if (oldValue != newValue)
                {
                    ringUC.Drug();
                }
            }
        }
        /// <summary>
        /// 圆圈图标
        /// </summary>
        public string icon
        {
            get { return (string)GetValue(iconProperty); }
            set { SetValue(iconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty iconProperty =
            DependencyProperty.Register("icon", typeof(string), typeof(RingUC));

        public void Drug()
        {
            
            //从7点到5点的圆
            LayOutGrid.Width = Math.Min(RenderSize.Width, RenderSize.Height);
            LayOutGrid.Height = Math.Min(RenderSize.Height, RenderSize.Width);
            
            double radius = LayOutGrid.Width / 2;

            if ((int)radius == 0)
            {
                return;
            }

            // 角度计算（从7点钟方向开始，覆盖300度）
            double startAngle = 120 * Math.PI / 180;       // 7点钟方向（120度）
            double endAngle;
            // 判断是否为大弧（超过180度）
            int IsLargeArc = (PercentValue % 100 * 3) >= 180 ? 1 : 0;

            if ((int)PercentValue == 100)
            {
                endAngle = (120 + 100 * 3) * Math.PI / 180; // 3度/百分比
                IsLargeArc = 1;
            }
            else
            {
                endAngle = (120 + (PercentValue % 100) * 3) * Math.PI / 180; // 3度/百分比
            }

            // 计算起点（7点钟方向）
            double startX = radius + (radius - 3) * Math.Cos(startAngle);
            double startY = radius + (radius - 3) * Math.Sin(startAngle);

            // 计算终点
            double endX = radius + (radius - 3) * Math.Cos(endAngle);
            double endY = radius + (radius - 3) * Math.Sin(endAngle);



            // 构建路径（从7点顺时针到动态终点）
            string pathStr = $"M{startX:0.00} {startY:0.00}A{radius - 3} {radius - 3} 0 {IsLargeArc} 1 {endX:0.00} {endY:0.00}";
            var converter = TypeDescriptor.GetConverter(typeof(Geometry));
            path.Data = (Geometry)converter.ConvertFrom(pathStr);


            //计算背景圆圈的角度
            double endBackAngle = (120 + 100 * 3) * Math.PI / 180; // 3度/百分比
            //计算背景圆圈的终点
            double endBackX = radius + (radius - 3) * Math.Cos(endBackAngle);
            double endBackY = radius + (radius - 3) * Math.Sin(endBackAngle);
            //构建路径
            string backPathStr = $"M{startX:0.00} {startY:0.00}A{radius - 3} {radius - 3} 0 1 1 {endBackX:0.00} {endBackY:0.00}";
            var converterBack = TypeDescriptor.GetConverter(typeof(Geometry));
            pathBack.Data = (Geometry)converterBack.ConvertFrom(backPathStr);
        }
    }
}

