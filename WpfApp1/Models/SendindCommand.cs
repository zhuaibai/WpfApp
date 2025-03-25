using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public class SendingCommand :INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 指令
        /// </summary>
        private string _command;
        public string Command
        {
            get { return _command; }
            set
            {
                if (_command != value)
                {
                    _command = value;
                    OnPropertyChanged(nameof(Command));
                }
            }
        }

        /// <summary>
        /// 返回字节
        /// </summary>
        private int _returnCount;
        public int ReturnCount
        {
            get { return _returnCount; }
            set
            {
                if (_returnCount != value)
                {
                    _returnCount = value;
                    OnPropertyChanged(nameof(ReturnCount));
                }
            }
        }

        /// <summary>
        /// 解析方法
        /// </summary>
        private string _analysisMethod;
        public string AnalysisMethod
        {
            get { return _analysisMethod; }
            set
            {
                if (_analysisMethod != value)
                {
                    _analysisMethod = value;
                    OnPropertyChanged(nameof(AnalysisMethod));
                }
            }
        }

        /// <summary>
        /// 是否启用
        /// </summary>
        private bool _enable;
        public bool Enable
        {
            get { return _enable; }
            set
            {
                if (_enable != value)
                {
                    _enable = value;
                    OnPropertyChanged(nameof(Enable));
                }
            }
        }
    }
}
