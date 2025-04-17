using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace WpfApp1.ViewModels
{
    public class ClockViewModel:BaseViewModel
    {
        private readonly DispatcherTimer _timer;
        private string _currentTime;


        public ClockViewModel()
        {
            // 初始化定时器（每秒更新一次）
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            UpdateTime();
        }

        public string CurrentTime
        {
            get => _currentTime;
            private set
            {
                if (_currentTime != value)
                {
                    _currentTime = value;
                    OnPropertyChanged(nameof(CurrentTime));
                }
            }
        }

        /// <summary>
        /// 事件(每秒一次
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTime();
        }

        /// <summary>
        /// 更新时间
        /// </summary>
        private void UpdateTime()
        {
            CurrentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");
        }

        /// <summary>
        /// 停止计时
        /// </summary>
        public void Dispose()
        {
            _timer.Stop();
            _timer.Tick -= Timer_Tick;
        }
    }
}
