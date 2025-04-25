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
    /// RingBackUC.xaml 的交互逻辑
    /// </summary>
    public partial class RingBackUC : UserControl
    {
        public RingBackUC()
        {
            InitializeComponent();
            SizeChanged += RingBackUC_SizeChanged;//界面大小改变
        }

        private void RingBackUC_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Drug();
        }


        /// <summary>
        /// 百分百 比如60
        /// </summary>
        public double PercentValue
        {
            get { return (double)GetValue(PrecentValueProperty); }
            set { SetValue(PrecentValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for PercentValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PrecentValueProperty =
            DependencyProperty.Register("PercentValue", typeof(double), typeof(RingUC));
        public void Drug()
        {
            //从7点到5点的圆
            LayOutGrid.Width = Math.Min(RenderSize.Width, RenderSize.Height);
            LayOutGrid.Height = Math.Min(RenderSize.Height, RenderSize.Width);
            double radius = LayOutGrid.Width / 2;

            // 角度计算（从7点钟方向开始，覆盖300度）
            double startAngle = 120 * Math.PI / 180;       // 7点钟方向（120度）
            double endAngle = (120 + (PercentValue % 100) * 3) * Math.PI / 180; // 3度/百分比

            // 计算起点（7点钟方向）
            double startX = radius + (radius - 3) * Math.Cos(startAngle);
            double startY = radius + (radius - 3) * Math.Sin(startAngle);

            // 计算终点
            double endX = radius + (radius - 3) * Math.Cos(endAngle);
            double endY = radius + (radius - 3) * Math.Sin(endAngle);

            // 判断是否为大弧（超过180度）
            int IsLargeArc = (PercentValue % 100 * 3) >= 180 ? 1 : 0;

            // 构建路径（从7点顺时针到动态终点）
            string pathStr = $"M{startX:0.00} {startY:0.00}A{radius - 3} {radius - 3} 0 {IsLargeArc} 1 {endX:0.00} {endY:0.00}";

            var converter = TypeDescriptor.GetConverter(typeof(Geometry));
            path.Data = (Geometry)converter.ConvertFrom(pathStr);
        }
    }
}
