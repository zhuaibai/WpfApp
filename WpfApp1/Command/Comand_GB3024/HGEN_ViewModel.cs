using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Command;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.Command.Comand_GB3024
{
    public class HGEN_ViewModel : BaseViewModel
    {
        //指令
        private string command = "HGEN\r";
        public string Command { get { return command; } }

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志
        public HGEN_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> _updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = _updateState;
            //设置系统时间
            Command_SetSystemTime = new RelayCommand(
                execute: () => SystemTimeOperation(),
                canExecute: () => !SystemTime_IsWorking // 增加处理状态检查
            );
        }


        #region 系统时间

        //系统时间
        private string _SystemTime;

        public string SystemTime
        {
            get { return _SystemTime; }
            set
            {
                _SystemTime = value;
                RaiseProperChanged(nameof(SystemTime));
            }
        }


        private bool SystemTime_IsWorking;


        //设置值
        private string _SystemTime_Inputs;

        public string SystemTime_Inputs
        {
            get { return _SystemTime_Inputs; }
            set
            {
                _SystemTime_Inputs = value;
                RaiseProperChanged(nameof(SystemTime_Inputs));
                Command_SetSystemTime.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetSystemTime { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void SystemTimeOperation()
        {
            try
            {
                SystemTime_IsWorking = true;
                // 禁用按钮
                Command_SetSystemTime.RaiseCanExecuteChanged();

                // 异步等待锁
                await _semaphore.WaitAsync();
                UpdateState("正在执行设置命令");
                //Status = "正在执行特殊操作...";

                // 暂停后台线程
                _pauseEvent.Reset();
                AddLog("已暂停后台通信");

                // 执行特殊操作（带超时保护）
                using var timeoutCts = new CancellationTokenSource(5000);
                await Task.Run(new Action(() =>
                {
                    //执行设置指令
                    Thread.Sleep(1000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("^S???DAT", GetTimeNow());

                })
                , timeoutCts.Token);
            }
            catch (OperationCanceledException)
            {
                AddLog("特殊操作执行超时");
            }
            finally
            {
                // 恢复后台线程
                _pauseEvent.Set();
                AddLog("恢复后台通信");
                SystemTime_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetSystemTime.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        /// <summary>
        /// 获取当前时间，并以特定格式返回
        /// </summary>
        /// <returns></returns>
        private string GetTimeNow()
        {
            string timeNow = DateTime.Now.ToString("yy/MM/dd/HH/mm/ss");
            if (string.IsNullOrEmpty(timeNow))
            {
                return "010719121212";
            }
            else
            {
                string result = timeNow.Replace("/","");
                
                return result;
            }
        }
            

        #endregion

        #region 通用方法
        // 输入验证&选择验证
        private bool Validate(string value)
        {
            //if (value== "WorkingModeSelectedOption")
            //{
            //    return !string.IsNullOrWhiteSpace(WorkingModeSelectedOption);
            //}
            //return true;
            switch (value)
            {
                case "SystemTime_Inputs":
                    return !string.IsNullOrWhiteSpace(SystemTime_Inputs);
                default:
                    return false;
            }
        }

        /// <summary>
        /// 对字符串进行解析
        /// </summary>
        /// <param name="value"></param>
        public void AnalyseStringToElement(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            string[] Values = value.Split(" ");
            try
            {
                string time = Values[0].Substring(1, 6);
                //系统时间
                SystemTime = $"20{time.Substring(0,2)}/{time.Substring(2,2)}/{time.Substring(4,2)} {Values[1]}";
            }
            catch (Exception ex)
            {
                return;
            }
        }

        /// <summary>
        /// 对属性值进行二次转换，换成命令指令符号
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string getSelectedToCommad(string value)
        {
            switch (value)
            {
                //设置工作模式
                //case "WorkingModeSelectedOption":
                //    if (string.IsNullOrWhiteSpace(WorkingModeSelectedOption))
                //    {
                //        return string.Empty;
                //    }
                //    else if (WorkingModeSelectedOption == "UTI")
                //    {
                //        return "00";
                //    }
                //    else if (WorkingModeSelectedOption == "SUB")
                //    {
                //        return "01";
                //    }
                //    return "";
                ////设置电池类型
                //case "BatteryTypeSelectedOption":
                //    if (string.IsNullOrWhiteSpace(BatteryTypeSelectedOption)) { return string.Empty; }
                //    else if (BatteryTypeSelectedOption == "AGM") { return "00"; }
                //    else if (BatteryTypeSelectedOption == "FLD") { return "01"; }
                //    else if (BatteryTypeSelectedOption == "USE") { return "02"; }
                //    else if (BatteryTypeSelectedOption == "LIA") { return "03"; }
                //    else if (BatteryTypeSelectedOption == "PYL") { return "04"; }
                //    else if (BatteryTypeSelectedOption == "TQF") { return "05"; }
                //    else if (BatteryTypeSelectedOption == "GRO") { return "06"; }
                //    else if (BatteryTypeSelectedOption == "LIB") { return "07"; }
                //    else if (BatteryTypeSelectedOption == "LIC") { return "08"; }
                //    else
                //        return "00";
                case "SBU":
                    return "";
                default:
                    return "";
            }
        }




        #endregion
    }
}
