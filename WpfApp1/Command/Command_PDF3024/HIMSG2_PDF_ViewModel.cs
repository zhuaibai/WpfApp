using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.Command.Command_PDF3024
{
    public class HIMSG2_PDF_ViewModel:BaseViewModel
    {
        //指令
        private string command = "HIMSG2\r";
        public string Command { get { return command; } }

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public HIMSG2_PDF_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = updateState;

            #region 初始化指令


            //逆变总功率
            Command_SetInvTotalPwr = new RelayCommand(
                execute: () => InvTotalPwrOperation(),
                canExecute: () => Validate(nameof(InvTotalPwr_Inputs)) && !InvTotalPwr_IsWorking // 增加处理状态检查
            );
            //MPPT总功率
            Command_SetMPPTTotalPwr = new RelayCommand(
               execute: () => MPPTTotalPwrOperation(),
               canExecute: () => Validate(nameof(MPPTTotalPwr_Inputs)) && !MPPTTotalPwr_IsWorking // 增加处理状态检查
            ); 
            
            #endregion
        }

        #region 逆变总功率
        private string _InvTotalPwr;

        public string InvTotalPwr
        {
            get { return _InvTotalPwr; }
            set
            {
                if (_InvTotalPwr != value)
                {
                    _InvTotalPwr = RemoveLeadingZeros(value);
                    this.RaiseProperChanged(nameof(InvTotalPwr));
                }
                
                
            }
        }


        private bool InvTotalPwr_IsWorking;


        //设置值
        private string _InvTotalPwr_Inputs;

        public string InvTotalPwr_Inputs
        {
            get { return _InvTotalPwr_Inputs; }
            set
            {
                _InvTotalPwr_Inputs = value;
                this.RaiseProperChanged(nameof(InvTotalPwr_Inputs));
                Command_SetInvTotalPwr.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetInvTotalPwr { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void InvTotalPwrOperation()
        {
            try
            {
                InvTotalPwr_IsWorking = true;
                // 禁用按钮
                Command_SetInvTotalPwr.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", InvTotalPwr_Inputs);

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
                InvTotalPwr_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetInvTotalPwr.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region MPPT总功率
        private string _MPPTTotalPwr;

        public string MPPTTotalPwr
        {
            get { return _MPPTTotalPwr; }
            set
            {
                _MPPTTotalPwr = RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(MPPTTotalPwr));
            }
        }


        private bool MPPTTotalPwr_IsWorking;


        //设置值
        private string _MPPTTotalPwr_Inputs;

        public string MPPTTotalPwr_Inputs
        {
            get { return _MPPTTotalPwr_Inputs; }
            set
            {
                _MPPTTotalPwr_Inputs = value;
                this.RaiseProperChanged(nameof(MPPTTotalPwr_Inputs));
                Command_SetMPPTTotalPwr.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetMPPTTotalPwr { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void MPPTTotalPwrOperation()
        {
            try
            {
                MPPTTotalPwr_IsWorking = true;
                // 禁用按钮
                Command_SetMPPTTotalPwr.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", MPPTTotalPwr_Inputs);

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
                MPPTTotalPwr_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetMPPTTotalPwr.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }

        #endregion

        #region 市电总功率
        private string _ACTotalPwr;

        public string ACTotalPwr
        {
            get { return _ACTotalPwr; }
            set
            {
                _ACTotalPwr = RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(ACTotalPwr));
            }
        }


        private bool ACTotalPwr_IsWorking;


        //设置值
        private string _ACTotalPwr_Inputs;

        public string ACTotalPwr_Inputs
        {
            get { return _ACTotalPwr_Inputs; }
            set
            {
                _ACTotalPwr_Inputs = value;
                this.RaiseProperChanged(nameof(ACTotalPwr_Inputs));
                Command_SetACTotalPwr.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetACTotalPwr { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ACTotalPwrOperation()
        {
            try
            {
                ACTotalPwr_IsWorking = true;
                // 禁用按钮
                Command_SetACTotalPwr.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", ACTotalPwr_Inputs);

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
                ACTotalPwr_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetACTotalPwr.RaiseCanExecuteChanged();
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
                case "InvTotalPwr_Inputs":
                    return !string.IsNullOrWhiteSpace(InvTotalPwr_Inputs);
                //PFC工作状态   
                case "MPPTTotalPwr_Inputs":
                    return !string.IsNullOrWhiteSpace(MPPTTotalPwr_Inputs);
                


                default:
                    return false;

            }
        }

        /// <summary>
        /// 移除字符串前边的0
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        string RemoveLeadingZeros(string input)
        {
            // 使用 TrimStart 方法移除前导的 0
            string result = input.TrimStart('0');
            // 如果结果为空字符串，说明原字符串全是 0，返回 "0"
            return string.IsNullOrEmpty(result) ? "0" : result;
        }

        /// <summary>
        /// 对字符串进行解析
        /// </summary>
        /// <param name="value"></param>
        public void AnalysisStringToElement(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                ReceiveException("0");
                return;
            }
            string[] Values = value.Split(" ");

            try
            {
                //逆变总电压
                InvTotalPwr = Values[0].Substring(1,5);
                //MPPT总功率
                MPPTTotalPwr = Values[1];
                //市电总功率
                ACTotalPwr = Values[2];
            }
            catch (Exception ex)
            {
                ReceiveException("HIMSG2异常");
            }
        }


        /// <summary>
        /// 接收异常使用方法
        /// </summary>
        /// <param name="exceptionDescription"></param>
        private void ReceiveException(string exceptionDescription)
        {

            //逆变总电压
            InvTotalPwr = exceptionDescription;
            //MPPT总功率
            MPPTTotalPwr = exceptionDescription;
            //市电总功率
            ACTotalPwr = exceptionDescription;
        }
        #endregion
    }
}
