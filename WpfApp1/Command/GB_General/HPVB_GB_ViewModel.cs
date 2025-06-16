using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Convert;
using WpfApp1.ViewModels;

namespace WpfApp1.Command.GB_General
{
    public class HPVB_GB_ViewModel : BaseViewModel
    {
        //指令
        private string command = "HGRID\r";
        public string Command { get { return command; } }

        public string MachineType;

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public HPVB_GB_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> _updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = _updateState;
        }

        #region PV2电压

        private string _PV2Volt;

        public string PV2Volt
        {
            get { return _PV2Volt; }
            set
            {
                _PV2Volt = value;
                this.RaiseProperChanged(nameof(PV2Volt));
            }
        }


        #endregion

        #region PV2电流
        private string _PV2Curr;

        public string PV2Curr
        {
            get { return _PV2Curr; }
            set
            {
                _PV2Curr = value;
                this.RaiseProperChanged(nameof(PV2Curr));
            }
        }
        #endregion

        #region PV2功率

        private string _PV2Pwr;

        public string PV2Pwr
        {
            get { return _PV2Pwr; }
            set
            {
                _PV2Pwr = value;
                this.RaiseProperChanged(nameof(PV2Pwr));
            }
        }


        #endregion


        #region 通用方法

        /// <summary>
        /// 对字符串进行解析
        /// </summary>
        /// <param name="value"></param>
        public void AnalysisStringToElement(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                ReceiveException("空");
                return;
            }
            string[] Values = value.Split(" ");

            try
            {
                //PV电压
                PV2Volt = Tools.RemoveLeadingZeros(Values[0].Substring(1, 5)) + "V";
                //PV电流
                PV2Curr = Tools.RemoveLeadingZeros(Values[1]) + "A";
                //PV功率
                PV2Pwr = Tools.RemoveLeadingZeros(Values[2]) + "W";

            }
            catch (Exception ex)
            {
                ReceiveException("HPV异常");
            }
        }


        /// <summary>
        /// 接收异常使用方法
        /// </summary>
        /// <param name="exceptionDescription"></param>
        private void ReceiveException(string exceptionDescription)
        {
            //PV电压
            PV2Volt = exceptionDescription;
            //PV电流
            PV2Curr = exceptionDescription;
            //PV功率
            PV2Pwr = exceptionDescription;
        }

        #endregion
    }
}
