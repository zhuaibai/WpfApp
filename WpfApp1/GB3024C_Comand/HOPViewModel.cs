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
        public HOPViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore,Action<string> addLog,Action<string> _updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState  = _updateState;
            
            Button1Command = new RelayCommand(
                execute: () => ExecuteButton1Operation(),
                canExecute: () => ValidateInput1() && !IsWorking // 增加处理状态检查
            );

        }
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


        private string _kame;
        public string Kame
        {
            get => _name;
            set
            {
                _kame = value;
                RaiseProperChanged(nameof(Kame));
            }
        }

        // 工作状态指示
        private bool _isWorking;
        public bool IsWorking
        {
            get => _isWorking;
            set
            {
                if (value == _isWorking) return;
                _isWorking = value;
                RaiseProperChanged(nameof(IsWorking));
            }
        }




        /// <summary>
        /// 对字符串进行解析
        /// </summary>
        /// <param name="value"></param>
        public void AnalysisStringToElement(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Name = value.Substring(1, 5);
                Kame = value.Substring(7, 4);
            }
        }
        // 示例1的属性和命令
        private string _input1 = string.Empty;
        public string Input1
        {
            get => _input1;
            set
            {
                _input1 = value;
                OnPropertyChanged();
                // 输入变化时更新按钮可用状态
                Button1Command.RaiseCanExecuteChanged();
            }
        }
        // 示例操作1（模拟耗时操作）
        private async void ExecuteButton1Operation()
        {
            try
            {
                IsWorking = true;
                // 禁用按钮
                Button1Command.RaiseCanExecuteChanged();

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
                    //Thread.Sleep(2000);
                    string receive = SerialCommunicationService.SendSettingCommand("PCVV", "23.8");

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
                IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Button1Command.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }


        }

        public RelayCommand Button1Command { get; }
        // 输入验证方法1：简单非空验证
        private bool ValidateInput1()
        {
            return !string.IsNullOrWhiteSpace(Input1);
        }

    }

}