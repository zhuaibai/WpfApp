using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp1.Convert;
using WpfApp1.Services;
using WpfApp1.ViewModels;

namespace WpfApp1.Command.Command_PDF302
{
    public class HCTMSG1_PDF_ViewModel:BaseViewModel
    {
        //指令
        private string command = "HCTMSG1\r";
        public string Command { get { return command; } }

        ManualResetEventSlim _pauseEvent;//线程的开启、暂停
        SemaphoreSlim _semaphore;        //异步竞争，资源锁
        Action<string> AddLog;           //添加日志委托
        Action<string> UpdateState;      //更新状态日志

        public HCTMSG1_PDF_ViewModel(ManualResetEventSlim pauseEvent, SemaphoreSlim semaphore, Action<string> addLog, Action<string> updateState)
        {
            _pauseEvent = pauseEvent;
            _semaphore = semaphore;
            AddLog = addLog;
            UpdateState = updateState;

            #region 初始化指令


            //CT电流
            Command_SetCTCurr = new RelayCommand(
                execute: () => CTCurrOperation(),
                canExecute: () => Validate(nameof(CTCurr_Inputs)) && !CTCurr_IsWorking // 增加处理状态检查
            );
            //CT功率
            Command_SetCTPwr = new RelayCommand(
               execute: () => CTPwrOperation(),
               canExecute: () => Validate(nameof(CTPwr_Inputs)) && !CTPwr_IsWorking // 增加处理状态检查
            );
            #endregion
        }



        #region CT电流

        private string _CTCurr;

        public string CTCurr
        {
            get { return _CTCurr; }
            set
            {
                _CTCurr = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(CTCurr));
            }
        }


        private bool CTCurr_IsWorking;


        //设置值
        private string _CTCurr_Inputs;

        public string CTCurr_Inputs
        {
            get { return _CTCurr_Inputs; }
            set
            {
                _CTCurr_Inputs = value;
                this.RaiseProperChanged(nameof(CTCurr_Inputs));
                Command_SetCTCurr.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetCTCurr { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void CTCurrOperation()
        {
            try
            {
                CTCurr_IsWorking = true;
                // 禁用按钮
                Command_SetCTCurr.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", CTCurr_Inputs);

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
                CTCurr_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetCTCurr.RaiseCanExecuteChanged();
                // 确保释放锁
                _semaphore.Release();
                UpdateState("设置指令已经执行完");
            }
        }



        #endregion

        #region CT功率

        //CT功率
        private string _CTPwr;

        public string CTPwr
        {
            get { return _CTPwr; }
            set
            {
                _CTPwr = Tools.RemoveLeadingZeros(value);
                this.RaiseProperChanged(nameof(CTPwr));
            }
        }


        private bool CTPwr_IsWorking;


        //设置值
        private string _CTPwr_Inputs;

        public string CTPwr_Inputs
        {
            get { return _CTPwr_Inputs; }
            set
            {
                _CTPwr_Inputs = value;
                this.RaiseProperChanged(nameof(CTPwr_Inputs));
                Command_SetCTPwr.RaiseCanExecuteChanged();
            }
        }


        public RelayCommand Command_SetCTPwr { get; }

        /// <summary>
        /// 点击设置
        /// </summary>
        private async void CTPwrOperation()
        {
            try
            {
                CTPwr_IsWorking = true;
                // 禁用按钮
                Command_SetCTPwr.RaiseCanExecuteChanged();

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
                    string receive = SerialCommunicationService.SendSettingCommand("设置指令", CTPwr_Inputs);

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
                CTPwr_IsWorking = false;
                //Status = "就绪";
                // 重新启用按钮
                Command_SetCTPwr.RaiseCanExecuteChanged();
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
                //CT电流   
                case "CTCurr_Inputs":
                    return !string.IsNullOrWhiteSpace(CTCurr_Inputs);
                //PFC工作状态   
                case "CTPwr_Inputs":
                    return !string.IsNullOrWhiteSpace(CTPwr_Inputs);
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
            if(value == "-1")
            {
                ReceiveException("CRC异常");
                return;
            }
            string[] Values = value.Split(" ");

            try
            {
                //CT电流
                CTCurr = Values[0].Substring(1,4);
                //CT功率
                CTPwr = Values[3];

            }
            catch (Exception ex)
            {
                ReceiveException("HCTMSG1解析异常");
            }
        }


        /// <summary>
        /// 接收异常使用方法
        /// </summary>
        /// <param name="exceptionDescription"></param>
        private void ReceiveException(string exceptionDescription)
        {
            //CT电流
            CTCurr = exceptionDescription;
            //CT功率
            CTPwr = exceptionDescription;
        }
        #endregion
    }
}
