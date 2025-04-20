using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.Command.Command_PDF3024
{
    public class HBAT_PDF_ViewModel : BaseViewModel
    {
        //指令
        private string command = "HOP\r";
        public string Command { get { return command; } }

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public HBAT_PDF_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = updateState;

            #region 初始化指令


            //母线电压
            Command_SetBusVolt = new RelayCommand(
                execute: () => BusVoltOperation(),
                canExecute: () => Validate(nameof(BusVolt_Inputs)) && !BusVolt_IsWorking // 增加处理状态检查
            );

            #endregion
        }

        

        #region 母线电压
        private string _BusVolt;

        public string BusVolt
        {
            get { return _BusVolt; }
            set
            {
                _BusVolt = value;
                this.RaiseProperChanged(nameof(BusVolt));
            }
        }


        private bool BusVolt_IsWorking;


        //设置值
        private string _BusVolt_Inputs;

        public string BusVolt_Inputs
        {
            get { return _BusVolt_Inputs; }
            set
            {
                _BusVolt_Inputs = value;
                this.RaiseProperChanged(nameof(BusVolt_Inputs));
                Command_SetBusVolt.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetBusVolt { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void BusVoltOperation()
        {
            try
            {
                BusVolt_IsWorking = true;
                // 禁用按钮
                Command_SetBusVolt.RaiseCanExecuteChanged();

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
                    Thread.Sleep(2000);//没有这个延时会报错
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", BusVolt_Inputs);

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
                BusVolt_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetBusVolt.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion


        #region 通用方法

        private bool Validate(string value)
        {
            switch (value)
            {
                //市电电压       
                case "BusVolt_Inputs":
                    return !string.IsNullOrWhiteSpace(BusVolt_Inputs);
                ////市电频率   
                //case "MainsFrequency_Inputs":
                //    return !string.IsNullOrWhiteSpace(MainsFrequency_Inputs);
                ////市电功率   
                //case "ACPower_Inputs":
                //    return !string.IsNullOrWhiteSpace(ACPower_Inputs);
                ////温度限制 
                //case "TempLimit_Inputs":
                //    return !string.IsNullOrWhiteSpace(TempLimit_Inputs);
                ////温度限制 
                //case "MaxInvPower_Inputs":
                //    return !string.IsNullOrWhiteSpace(MaxInvPower_Inputs);


                default:
                    return false;

            }
        }
        #endregion
    }
}
