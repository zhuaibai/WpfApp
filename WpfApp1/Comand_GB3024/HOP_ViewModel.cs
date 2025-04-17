using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WpfApp1.Command;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.GB3024C_Comand
{
    public class HOPViewModel : BaseViewModel
    {
        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志


        //指令
        private string command = "HOP\r";

        public string Command { get { return command; } }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                RaiseProperChanged(nameof(Name));
            }
        }

        public HOPViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> _updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = _updateState;

            //输出电压
            Command_SetOutputVoltage = new RelayCommand(
                execute: () => OutputVoltageOperation(),
                canExecute: () => Validate(nameof(OutputVoltage_Inputs)) && !OutputVoltage_IsWorking // 增加处理状态检查
            );

        }

        #region 输出电压

        private string _OutputVoltage;

        public string OutputVoltage
        {
            get { return _OutputVoltage; }
            set
            {
                _OutputVoltage = value;
                this.RaiseProperChanged(nameof(OutputVoltage));
            }
        }


        private bool OutputVoltage_IsWorking;


        //设置值
        private string _OutputVoltage_Inputs;

        public string OutputVoltage_Inputs
        {
            get { return _OutputVoltage_Inputs; }
            set
            {
                _OutputVoltage_Inputs = value;
                this.RaiseProperChanged(nameof(OutputVoltage_Inputs));
                Command_SetOutputVoltage.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _OutputVoltageOptions = new List<string> { "220", "230","240" };

        public List<string> OutputVoltageOptions
        {
            get { return _OutputVoltageOptions; }
            set
            {
                _OutputVoltageOptions = value;
                this.RaiseProperChanged(nameof(OutputVoltageOptions));
            }
        }

        public RelayCommand Command_SetOutputVoltage { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void OutputVoltageOperation()
        {
            try
            {
                OutputVoltage_IsWorking = true;
                // 禁用按钮
                Command_SetOutputVoltage.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("V", OutputVoltage_Inputs);

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
                OutputVoltage_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetOutputVoltage.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion






        /// <summary>
        /// 对字符串进行解析
        /// </summary>
        /// <param name="value"></param>
        public void AnalysisStringToElement(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            string[] Values = value.Split(" ");

            try
            {
                //输出电压
                OutputVoltage = Values[0].Substring(1, 5);
            }
            catch (Exception ex)
            {

            }
        }


        // 输入验证&选择验证
        private bool Validate(string value)
        {
            switch (value)
            {
                case "OutputVoltage_Inputs":
                    return !string.IsNullOrWhiteSpace(OutputVoltage_Inputs);
                default:
                    return false;
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
                //输出电压
                case "OutputVoltage_Inputs":
                    if (string.IsNullOrWhiteSpace(OutputVoltage_Inputs))
                    {
                        return string.Empty;
                    }
                    else if (OutputVoltage_Inputs == "UTI")
                    {
                        return "00";
                    }
                    else if (OutputVoltage_Inputs == "SUB")
                    {
                        return "01";
                    }
                    return "";
                default:
                    return string.Empty;

            }
        }
    }
}