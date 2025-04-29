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
    public class HEEP3_PDF_ViewModel:BaseViewModel
    {
        //指令
        private string command = "HEEP3\r";
        public string Command { get { return command; } }

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public HEEP3_PDF_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = updateState;

            #region 初始化指令


            //调零功率
            Command_SetZeroAdjPwr = new RelayCommand(
                execute: () => ZeroAdjPwrOperation(),
                canExecute: () => Validate(nameof(ZeroAdjPwr_Inputs)) && !ZeroAdjPwr_IsWorking // 增加处理状态检查
            );
            //PFC工作状态
            Command_SetPFCStatus = new RelayCommand(
               execute: () => PFCStatusOperation(),
               canExecute: () => Validate(nameof(PFCStatus_Inputs)) && !PFCStatus_IsWorking // 增加处理状态检查
            );
            #endregion
        }



        #region 调零功率

        private string _ZeroAdjPwr2;

        public string ZeroAdjPwr2
        {
            get { return _ZeroAdjPwr2; }
            set
            {
                _ZeroAdjPwr2 = value+"W";
                this.RaiseProperChanged(nameof(ZeroAdjPwr2));
            }
        }


        //调零功率
        private string _ZeroAdjPwr;

        public string ZeroAdjPwr
        {
            get { return _ZeroAdjPwr; }
            set
            {
                _ZeroAdjPwr = Tools.RemoveLeadingZeros(value);
                ZeroAdjPwr2 = _ZeroAdjPwr;
                this.RaiseProperChanged(nameof(ZeroAdjPwr));
            }
        }


        private bool ZeroAdjPwr_IsWorking;


        //设置值
        private string _ZeroAdjPwr_Inputs;

        public string ZeroAdjPwr_Inputs
        {
            get { return _ZeroAdjPwr_Inputs; }
            set
            {
                _ZeroAdjPwr_Inputs = value;
                this.RaiseProperChanged(nameof(ZeroAdjPwr_Inputs));
                Command_SetZeroAdjPwr.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetZeroAdjPwr { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void ZeroAdjPwrOperation()
        {
            try
            {
                ZeroAdjPwr_IsWorking = true;
                // 禁用按钮
                Command_SetZeroAdjPwr.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("EZCTP", ZeroAdjPwr_Inputs);

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
                ZeroAdjPwr_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetZeroAdjPwr.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }


        #endregion

        #region PFC工作状态

        //PFC工作状态
        private string _PFCStatus;

        public string PFCStatus
        {
            get { return _PFCStatus; }
            set
            {
                if (value == "0")
                {
                    _PFCStatus = App.GetText("关闭");
                }
                else if (value == "1") { _PFCStatus = App.GetText("开启"); }
                else
                    _PFCStatus = value;
                this.RaiseProperChanged(nameof(PFCStatus));
            }
        }


        private bool PFCStatus_IsWorking;


        //设置值
        private string _PFCStatus_Inputs;

        public string PFCStatus_Inputs
        {
            get { return _PFCStatus_Inputs; }
            set
            {
                _PFCStatus_Inputs = value;
                this.RaiseProperChanged(nameof(PFCStatus_Inputs));
                Command_SetPFCStatus.RaiseCanExecuteChanged();
            }
        }

        //下拉选项
        private List<string> _PFCStatusOptions = new List<string> { "开启", "关闭" };

        public List<string> PFCStatusOptions
        {
            get { return _PFCStatusOptions; }
            set
            {
                _PFCStatusOptions = value;
                this.RaiseProperChanged(nameof(PFCStatusOptions));
            }
        }

        public RelayCommand Command_SetPFCStatus { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void PFCStatusOperation()
        {
            try
            {
                PFCStatus_IsWorking = true;
                // 禁用按钮
                Command_SetPFCStatus.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", PFCStatus_Inputs);

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
                PFCStatus_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetPFCStatus.RaiseCanExecuteChanged();
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
                //调零功率    
                case "ZeroAdjPwr_Inputs":
                    return !string.IsNullOrWhiteSpace(ZeroAdjPwr_Inputs);
                //PFC工作状态   
                case "PFCStatus_Inputs":
                    return !string.IsNullOrWhiteSpace(PFCStatus_Inputs);
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
                //调零功率
                ZeroAdjPwr = Values[3];
                //PFC工作状态
                //PFCStatus = Values[6].Substring(0, 1);

            }
            catch (Exception ex)
            {
                ReceiveException("HEEP3异常");
            }
        }

        /// <summary>
        /// 接收异常使用方法
        /// </summary>
        /// <param name="exceptionDescription"></param>
        private void ReceiveException(string exceptionDescription)
        {
            //调零功率
            ZeroAdjPwr = exceptionDescription;
            //PFC工作状态
            //PFCStatus = Values[6].Substring(0, 1);
        }
        #endregion
    }
}
