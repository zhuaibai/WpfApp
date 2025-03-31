using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace WpfApp1.Behavior
{
    public static class ScrollBehavior
    {
        // 修改为可绑定的附加属性
        public static readonly DependencyProperty AdjustScrollOnAddProperty =
            DependencyProperty.RegisterAttached(
                "AdjustScrollOnAdd",
                typeof(bool),
                typeof(ScrollBehavior),
                new PropertyMetadata(false, OnAdjustScrollOnAddChanged));



        public static bool GetAdjustScrollOnAdd(DependencyObject obj) =>
            (bool)obj.GetValue(AdjustScrollOnAddProperty);

        public static void SetAdjustScrollOnAdd(DependencyObject obj, bool value) =>
            obj.SetValue(AdjustScrollOnAddProperty, value);

        // 属性变化时监听集合变化
        private static void OnAdjustScrollOnAddChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBox listBox) return;

            if ((bool)e.NewValue)
            {
                listBox.Loaded += ListBox_Loaded;
            }
            else
            {
                listBox.Loaded -= ListBox_Loaded;
            }
        }

        private static void ListBox_Loaded(object sender, RoutedEventArgs e)
        {
            var listBox = (ListBox)sender;
            if (listBox.ItemsSource is not INotifyCollectionChanged collection) return;

            ScrollViewer? scrollViewer = null;
            double oldOffset = 0;
            double oldExtent = 0;

            // 查找 ScrollViewer
            void FindScrollViewer()
            {
                if (scrollViewer != null) return;
                scrollViewer = FindVisualChild<ScrollViewer>(listBox);
            }

            // 监听集合变化
            collection.CollectionChanged += (s, args) =>
            {
                // 仅在启用滚动调整时执行逻辑
                if (!GetAdjustScrollOnAdd(listBox)) return; // 关键修改点

                if (args.Action != NotifyCollectionChangedAction.Add || args.NewStartingIndex != 0) return;

                FindScrollViewer();
                if (scrollViewer == null) return;

                // 记录旧状态
                oldOffset = scrollViewer.VerticalOffset;
                oldExtent = scrollViewer.ExtentHeight;

                // 等待布局更新后调整滚动条
                listBox.Dispatcher.BeginInvoke(new Action(() =>
                {
                    double newExtent = scrollViewer.ExtentHeight;
                    double delta = newExtent - oldExtent;
                    scrollViewer.ScrollToVerticalOffset(oldOffset + delta);
                }), System.Windows.Threading.DispatcherPriority.Background);
            };
        }

        // 递归查找 ScrollViewer
        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                    return result;
                var descendant = FindVisualChild<T>(child);
                if (descendant != null)
                    return descendant;
            }
            return null;
        }
    }
}
