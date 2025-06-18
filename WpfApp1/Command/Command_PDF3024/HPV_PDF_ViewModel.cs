using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Convert;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.Command.Command_PDF3024
{
    public class HPV_PDF_ViewModel:BaseViewModel
    {

        //指令
        private string command = "HPV\r";
        public string Command { get { return command; } }

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public HPV_PDF_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = updateState;

            #region 初始化指令

            //PV电压
            Command_SetPVVolt = new RelayCommand(
                execute: () => PVVoltOperation(),
                canExecute: () => Validate(nameof(PVVolt_Inputs)) && !PVVolt_IsWorking // 增加处理状态检查
            );
            //PV电流
            Command_SetPVCurr = new RelayCommand(
               execute: () => PVCurrOperation(),
               canExecute: () => Validate(nameof(PVCurr_Inputs)) && !PVCurr_IsWorking // 增加处理状态检查
            );
            //PV功率
            Command_SetPVPwr = new RelayCommand(
               execute: () => PVPwrOperation(),
               canExecute: () => Validate(nameof(PVPwr_Inputs)) && !PVPwr_IsWorking // 增加处理状态检查
            );
            #endregion
        }


        #region PV电压

        //PV电压
        private string _PVVolt;

        public string PVVolt
        {
            get { return _PVVolt; }
            set
            {
                _PVVolt = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(PVVolt));
            }
        }


        private bool PVVolt_IsWorking;


        //设置值
        private string _PVVolt_Inputs;

        public string PVVolt_Inputs
        {
            get { return _PVVolt_Inputs; }
            set
            {
                _PVVolt_Inputs = value;
                this.RaiseProperChanged(nameof(PVVolt_Inputs));
                Command_SetPVVolt.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetPVVolt { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void PVVoltOperation()
        {
            try
            {
                PVVolt_IsWorking = true;
                // 禁用按钮
                Command_SetPVVolt.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", PVVolt_Inputs);

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
                PVVolt_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetPVVolt.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region PV电流

        //PV电流
        private string _PVCurr;

        public string PVCurr
        {
            get { return _PVCurr; }
            set
            {
                _PVCurr = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(PVCurr));
            }
        }


        private bool PVCurr_IsWorking;


        //设置值
        private string _PVCurr_Inputs;

        public string PVCurr_Inputs
        {
            get { return _PVCurr_Inputs; }
            set
            {
                _PVCurr_Inputs = value;
                this.RaiseProperChanged(nameof(PVCurr_Inputs));
                Command_SetPVCurr.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetPVCurr { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void PVCurrOperation()
        {
            try
            {
                PVCurr_IsWorking = true;
                // 禁用按钮
                Command_SetPVCurr.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", PVCurr_Inputs);

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
                PVCurr_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetPVCurr.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region PV功率

        private string _PVPwr;

        public string PVPwr
        {
            get { return _PVPwr; }
            set
            {
                _PVPwr = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(PVPwr));
            }
        }


        private bool PVPwr_IsWorking;


        //设置值
        private string _PVPwr_Inputs;

        public string PVPwr_Inputs
        {
            get { return _PVPwr_Inputs; }
            set
            {
                _PVPwr_Inputs = value;
                this.RaiseProperChanged(nameof(PVPwr_Inputs));
                Command_SetPVPwr.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetPVPwr { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void PVPwrOperation()
        {
            try
            {
                PVPwr_IsWorking = true;
                // 禁用按钮
                Command_SetPVPwr.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", PVPwr_Inputs);

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
                PVPwr_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetPVPwr.RaiseCanExecuteChanged();
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
                case "PVVolt_Inputs":
                    return !string.IsNullOrWhiteSpace(PVVolt_Inputs);
                //PFC工作状态   
                case "PVCurr_Inputs":
                    return !string.IsNullOrWhiteSpace(PVCurr_Inputs);
                //PV功率   
                case "PVPwr_Inputs":
                    return !string.IsNullOrWhiteSpace(PVPwr_Inputs);
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
            if(value == "-1")
            {
                ReceiveException("CRC异常");
                return;
            }
            string[] Values = value.Split(" ");

            try
            {
                //PV电压
                PVVolt = Values[0].Substring(1,5);
                //PV电流
                PVCurr = Values[1];
                //PV功率
                PVPwr = Values[2];

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
            PVVolt = exceptionDescription;
            //PV电流
            PVCurr = exceptionDescription;
            //PV功率
            PVPwr = exceptionDescription;
        }
        #endregion
    }
}
